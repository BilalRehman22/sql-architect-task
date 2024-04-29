namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Settings
{
    public class CognitiveSearchSettings
    {
        public required string ServiceName { get; set; }
        public required string EndpointUriString { get; set; }
        public required string IndexName { get; set; }
        public required string IndexerName { get; set; }
        public required string ApiKey { get; set; }
        public required string AdminApiKey { get; set; }
    }
}
