using EmpowerId.ProductCatalog.ConsoleApp.Data.Models;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services
{
    public interface IProductAttributeSearchService
    {
        IEnumerable<Product> Search(long? productId, string? productName, string? categoryName,
            string? description, decimal? priceMin, decimal? priceMax, string? imageUrl, DateTime? dateAdded);
    }
}