using Dapper;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Helpers;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using EmpowerId.ProductCatalog.ConsoleApp.Data.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Data.Queries;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EmpowerId.ProductCatalog.ConsoleApp.Data.Handlers.QueryHandlers
{
    public class GetProductsQueryHandler : IGetProductsQueryHandler
    {
        private readonly DatabaseSettings _dbSettings;
        private readonly AppSettings _appSettings;
        private readonly IAuthHelper _authHelper;

        public GetProductsQueryHandler(IOptions<DatabaseSettings> dbSettings, IOptions<AppSettings> appSettings, IAuthHelper authHelper)
        {
            _dbSettings = dbSettings?.Value ?? throw new Exception("Database settings not configured");
            _appSettings = appSettings?.Value ?? throw new Exception("App settings not configured");
            _authHelper = authHelper;
        }

        public IEnumerable<Product> Handle(GetProductsQuery query)
        {
            using (var connection = new SqlConnection(_dbSettings.SqlConnectionString))
            {
                connection.AccessToken = _authHelper.GetDatabaseAccessTokenAsync().GetAwaiter().GetResult();
                connection.Open();

                var sql = $"SELECT TOP {_appSettings.SearchPageSize} p.product_id ProductId, p.product_name ProductName, c.category_name CategoryName, " +
                          "p.price, p.description, p.image_url ImageUrl, p.date_added DateAdded " +
                          "FROM products p " +
                          "LEFT JOIN categories c ON p.category_id = c.category_id " +
                          "WHERE 1 = 1 ";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrWhiteSpace(query.ProductName))
                {
                    sql += "AND p.product_name LIKE @ProductName ";
                    parameters.Add("ProductName", "%" + query.ProductName + "%");
                }

                if (!string.IsNullOrWhiteSpace(query.CategoryName))
                {
                    sql += "AND c.category_name LIKE @CategoryName ";
                    parameters.Add("CategoryName", "%" + query.CategoryName + "%");
                }

                if (query.PriceMinimum.HasValue)
                {
                    sql += "AND p.price >= @MinPrice ";
                    parameters.Add("MinPrice", query.PriceMinimum);
                }

                if (query.PriceMaximum.HasValue)
                {
                    sql += "AND p.price <= @MaxPrice ";
                    parameters.Add("MaxPrice", query.PriceMaximum);
                }

                if (!string.IsNullOrWhiteSpace(query.Description))
                {
                    sql += "AND p.description LIKE @Description ";
                    parameters.Add("Description", "%" + query.Description + "%");
                }

                if (!string.IsNullOrWhiteSpace(query.ImageUrl))
                {
                    sql += "AND p.image_url LIKE @ImageUrl ";
                    parameters.Add("ImageUrl", "%" + query.ImageUrl + "%");
                }

                if (query.DateAdded.HasValue)
                {
                    sql += "AND p.date_added = @DateAdded ";
                    parameters.Add("DateAdded", query.DateAdded);
                }

                var products = connection.Query<Product>(sql, parameters);

                return products;
            }
        }
    }
}
