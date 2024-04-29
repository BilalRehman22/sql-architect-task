using EmpowerId.ProductCatalog.ConsoleApp.Core.Helpers;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace EmpowerId.ProductCatalog.ConsoleApp.BackgroundServices
{
    public class ChangeDataCaptureBackgroundService : BackgroundService
    {
        private const string CheckpointFilePath = "cdcTimestamp.json";
        private const string CdcQuery = 
            "SELECT * FROM {0} " +
            "WHERE __$operation IN (1, 2, 4) " +
            "AND __$start_lsn > sys.fn_cdc_map_time_to_lsn('smallest greater than or equal', '{1}')";
        
        private readonly DatabaseSettings _dbSettings;
        private readonly IAuthHelper _authHelper;
        private readonly ILogger<ChangeDataCaptureBackgroundService> _logger;

        public ChangeDataCaptureBackgroundService(
            IOptions<DatabaseSettings> dbSettings,
            IAuthHelper authHelper,
            ILogger<ChangeDataCaptureBackgroundService> logger)
        {
            _dbSettings = dbSettings?.Value ?? throw new Exception("Database settings not configured");
            _authHelper = authHelper;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Running Change Data Capture process..");

                    await ProcessChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task ProcessChangesAsync(CancellationToken cancellationToken)
        {
            var changeCounter = InitializeChangeCounter();
            var lastProcessedTimestamp = await LoadLastProcessedTimestampAsync();

            var productsCdcQuery = string.Format(CdcQuery, "cdc.dbo_products_CT", lastProcessedTimestamp);
            var categoriesCdcQuery = string.Format(CdcQuery, "cdc.dbo_categories_CT", lastProcessedTimestamp);

            var accessToken = await _authHelper.GetDatabaseAccessTokenAsync();

            using (var sourceConnection = new SqlConnection(_dbSettings.SqlConnectionString))
            using (var destinationConnection = new SqlConnection(_dbSettings.ExternalDbSqlConnectionString))
            {
                sourceConnection.AccessToken = accessToken;
                destinationConnection.AccessToken = accessToken;

                await sourceConnection.OpenAsync(cancellationToken);
                await destinationConnection.OpenAsync(cancellationToken);
                
                using var transaction = (SqlTransaction) await destinationConnection.BeginTransactionAsync();

                // Create commands
                using (var productsCommand = new SqlCommand(productsCdcQuery, sourceConnection))
                using (var categoriesCommand = new SqlCommand(categoriesCdcQuery, sourceConnection))
                {
                    try
                    {
                        using (var productsReader = productsCommand.ExecuteReader())
                        {
                            while (await productsReader.ReadAsync(cancellationToken))
                            {
                                await ApplyDataChangeIntoDestinationAsync("products", productsReader, destinationConnection, transaction, changeCounter, cancellationToken);
                            }
                        }

                        using (var categoriesReader = categoriesCommand.ExecuteReader())
                        {
                            while (await categoriesReader.ReadAsync(cancellationToken))
                            {
                                await ApplyDataChangeIntoDestinationAsync("categories", categoriesReader, destinationConnection, transaction, changeCounter, cancellationToken);
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                await UpdateLastProcessedTimestampAsync(DateTime.Now);

                var changeSummary = JsonSerializer.Serialize(changeCounter);
                _logger.LogInformation("Change Data Capture process completed successfuly.");
                _logger.LogInformation("Count of changes applied: \n{changeSummary}", changeSummary);
            }
        }

        private Dictionary<string, Dictionary<ChangeType, int>> InitializeChangeCounter()
        {
            var counter = new Dictionary<string, Dictionary<ChangeType, int>>();
            
            counter["products"] = new Dictionary<ChangeType, int>()
            {
                { ChangeType.Insert, 0 },
                { ChangeType.Update, 0 },
                { ChangeType.Delete, 0 },
            };

            counter["categories"] = new Dictionary<ChangeType, int>()
            {
                { ChangeType.Insert, 0 },
                { ChangeType.Update, 0 },
                { ChangeType.Delete, 0 },
            };

            return counter;
        }

        private async Task<string> LoadLastProcessedTimestampAsync()
        {
            var lastProcessedTimestamp = DateTime.MinValue;

            try
            {
                if (File.Exists(CheckpointFilePath))
                {
                    var json = await File.ReadAllTextAsync(CheckpointFilePath);
                    lastProcessedTimestamp = JsonSerializer.Deserialize<DateTime>(json);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading last processed timestamp: {ex.Message}");
            }

            return lastProcessedTimestamp.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private async Task UpdateLastProcessedTimestampAsync(DateTime timestamp)
        {
            try
            {
                var json = JsonSerializer.Serialize(timestamp);
                await File.WriteAllTextAsync(CheckpointFilePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating last processed timestamp: {ex.Message}");
            }
        }

        private async Task ApplyDataChangeIntoDestinationAsync(string tableName, SqlDataReader reader, SqlConnection destinationConnection,
            SqlTransaction transaction, Dictionary<string, Dictionary<ChangeType, int>> changeCounter, CancellationToken cancellationToken)
        {
            // Check the operation type
            var operation = reader.GetInt32(reader.GetOrdinal("__$operation"));
            
            if (!Enum.IsDefined(typeof(ChangeType), operation))
                return;

            var changeType = (ChangeType)operation;

            switch (changeType)
            {
                case ChangeType.Delete:
                    await DeleteRecordAsync(reader, destinationConnection, transaction, tableName, cancellationToken);
                    break;
                case ChangeType.Insert:
                    await InsertRecordAsync(reader, destinationConnection, transaction, tableName, cancellationToken);
                    break;
                case ChangeType.Update:
                    await UpdateRecordAsync(reader, destinationConnection, transaction, tableName, cancellationToken);
                    break;
                default:
                    throw new Exception("Unsupported change type encountered."); // should not happen [until it does :(]
            }

            changeCounter[tableName][changeType]++;
        }

        private async Task InsertRecordAsync(SqlDataReader reader, SqlConnection destinationConnection, SqlTransaction transaction, string tableName, CancellationToken cancellationToken)
        {
            // Define the insert query based on the table name and schema
            var insertQuery = tableName switch
            {
                "products" => "INSERT INTO EXTERNALDB.Products (product_id, product_name, category_id, price, last_update_date) VALUES (@product_id, @product_name, @category_id, @price, @last_update_date)",
                "categories" => "INSERT INTO EXTERNALDB.Categories (category_id, category_name, last_update_date) VALUES (@category_id, @category_name, @last_update_date)",
                _ => throw new NotSupportedException($"Table '{tableName}' is not supported."),
            };

            // Set parameters based on table schema
            using var insertCommand = new SqlCommand(insertQuery, destinationConnection, transaction);
            if (tableName == "products")
            {
                insertCommand.Parameters.AddWithValue("@product_id", reader["product_id"]);
                insertCommand.Parameters.AddWithValue("@product_name", reader["product_name"]);
                insertCommand.Parameters.AddWithValue("@category_id", reader["category_id"]);
                insertCommand.Parameters.AddWithValue("@price", reader["price"]);
                insertCommand.Parameters.AddWithValue("@last_update_date", DateTime.Now);
            }
            else if (tableName == "categories")
            {
                insertCommand.Parameters.AddWithValue("@category_id", reader["category_id"]);
                insertCommand.Parameters.AddWithValue("@category_name", reader["category_name"]);
                insertCommand.Parameters.AddWithValue("@last_update_date", DateTime.Now);
            }

            // Execute the insert command asynchronously with cancellation token
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        private async Task UpdateRecordAsync(SqlDataReader reader, SqlConnection destinationConnection, SqlTransaction transaction, string tableName, CancellationToken cancellationToken)
        {
            // Define the update query based on the table name and schema
            var updateQuery = tableName switch
            {
                "products" => "UPDATE EXTERNALDB.Products SET product_name = @product_name, category_id = @category_id, price = @price, last_update_date = @last_update_date WHERE product_id = @product_id",
                "categories" => "UPDATE EXTERNALDB.Categories SET category_name = @category_name, last_update_date = @last_update_date WHERE category_id = @category_id",
                _ => throw new NotSupportedException($"Table '{tableName}' is not supported."),
            };

            // Set parameters based on table schema
            using var updateCommand = new SqlCommand(updateQuery, destinationConnection, transaction);
            if (tableName == "products")
            {
                updateCommand.Parameters.AddWithValue("@product_id", reader["product_id"]);
                updateCommand.Parameters.AddWithValue("@product_name", reader["product_name"]);
                updateCommand.Parameters.AddWithValue("@category_id", reader["category_id"]);
                updateCommand.Parameters.AddWithValue("@price", reader["price"]);
                updateCommand.Parameters.AddWithValue("@last_update_date", DateTime.Now);
            }
            else if (tableName == "categories")
            {
                updateCommand.Parameters.AddWithValue("@category_id", reader["category_id"]);
                updateCommand.Parameters.AddWithValue("@category_name", reader["category_name"]);
                updateCommand.Parameters.AddWithValue("@last_update_date", DateTime.Now);
            }

            // Execute the update command asynchronously with cancellation token
            await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        private async Task DeleteRecordAsync(SqlDataReader reader, SqlConnection destinationConnection, SqlTransaction transaction, string tableName, CancellationToken cancellationToken)
        {
            // Define the delete query based on the table name and schema
            var deleteQuery = tableName switch
            {
                "products" => "DELETE FROM EXTERNALDB.Products WHERE product_id = @product_id",
                "categories" => "DELETE FROM EXTERNALDB.Categories WHERE category_id = @category_id",
                _ => throw new NotSupportedException($"Table '{tableName}' is not supported."),
            };

            // Set parameters based on table schema
            using var deleteCommand = new SqlCommand(deleteQuery, destinationConnection, transaction);
            if (tableName == "products")
            {
                deleteCommand.Parameters.AddWithValue("@product_id", reader["product_id"]);
            }
            else if (tableName == "categories")
            {
                deleteCommand.Parameters.AddWithValue("@category_id", reader["category_id"]);
            }

            // Execute the delete command asynchronously with cancellation token
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        private enum ChangeType
        {
            Delete = 1,
            Insert = 2,
            Update = 4,
        }
    }
}
