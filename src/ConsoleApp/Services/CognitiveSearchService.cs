using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure;
using EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos;
using Azure.Search.Documents;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using Microsoft.Extensions.Options;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services
{
    public class CognitiveSearchService : ICognitiveSearchService
    {
        private readonly CognitiveSearchSettings _cognitiveSearchSettings;
        private readonly AppSettings _appSettings;

        public CognitiveSearchService(IOptions<CognitiveSearchSettings> cognitiveSearchSettings, IOptions<AppSettings> appSettings)
        {
            _cognitiveSearchSettings = cognitiveSearchSettings?.Value ?? throw new Exception("Cognitive Search settings not configured");
            _appSettings = appSettings?.Value ?? throw new Exception("App settings not configured");
        }

        public Result<bool, HttpError> RunIndexer()
        {
            var credentials = new AzureKeyCredential(_cognitiveSearchSettings.AdminApiKey);
            var indexer = new SearchIndexerClient(new Uri(_cognitiveSearchSettings.EndpointUriString), credentials);

            var response = indexer.RunIndexer(_cognitiveSearchSettings.IndexerName);
            return response.Result();
        }

        public Result<SearchResults<CognitiveSearchResult>, HttpError> Search(string searchTerm)
        {
            // Create a SearchClient instance
            var credentials = new AzureKeyCredential(_cognitiveSearchSettings.ApiKey);

            var searchClient = new SearchClient(
                new Uri(_cognitiveSearchSettings.EndpointUriString),
                _cognitiveSearchSettings.IndexName,
                credentials);

            // Execute a search query
            var options = new SearchOptions()
            {
                IncludeTotalCount = true,
                Filter = "",
                Size = _appSettings.SearchPageSize
            };

            options.Select.Add("product_id");
            options.Select.Add("product_name");
            options.Select.Add("description");
            options.Select.Add("category_name");

            var searchResults = searchClient.Search<CognitiveSearchResult>(searchTerm, options);
            return searchResults.Result();
        }
    }
}
