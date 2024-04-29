using EmpowerId.ProductCatalog.ConsoleApp.Data.Handlers.QueryHandlers;
using EmpowerId.ProductCatalog.ConsoleApp.Data.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Data.Queries;
using EmpowerId.ProductCatalog.ConsoleApp.Views;
using Microsoft.Extensions.Logging;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services
{
    public class ProductAttributeSearchService : IProductAttributeSearchService
    {
        private readonly IGetProductsQueryHandler _queryHandler;
        private readonly ILogger<ProductAttributeSearchService> _logger;

        public ProductAttributeSearchService(IGetProductsQueryHandler queryHandler, ILogger<ProductAttributeSearchService> logger)
        {
            _queryHandler = queryHandler;
            _logger = logger;
        }

        public IEnumerable<Product> Search(long? productId, string? productName, string? categoryName,
            string? description, decimal? priceMin, decimal? priceMax, string? imageUrl, DateTime? dateAdded)
        {
            var query = new GetProductsQuery
            {
                ProductId = productId,
                ProductName = productName,
                CategoryName = categoryName,
                Description = description,
                PriceMinimum = priceMin,
                PriceMaximum = priceMax,
                DateAdded = dateAdded,
                ImageUrl = imageUrl,
            };

            _logger.LogInformation("Searching for products based on provided attribute values {attributes}", query);

            return _queryHandler.Handle(query);
        }
    }
}
