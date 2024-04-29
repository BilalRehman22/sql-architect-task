namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Settings
{
    public class DatabaseSettings
    {
        public required string SqlConnectionString { get; set; }
        public required string ExternalDbSqlConnectionString { get; set; }
        public required string ExternalDbSchemaName { get; set; }
        public required string ResourceId { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}
