using Azure.Search.Documents.Indexes;
using System.Text.Json.Serialization;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos
{
    public class CognitiveSearchResult
    {
        [SearchableField]
        [JsonPropertyName("product_id")]
        public string? ProductId { get; set; }

        [SearchableField]
        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        [SearchableField]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [SearchableField(IsFilterable = true)]
        [JsonPropertyName("category_name")]
        public string? CategoryName { get; set; }
    }
}
