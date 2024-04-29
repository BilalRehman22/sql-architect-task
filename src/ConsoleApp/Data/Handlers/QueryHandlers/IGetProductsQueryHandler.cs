using EmpowerId.ProductCatalog.ConsoleApp.Data.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Data.Queries;

namespace EmpowerId.ProductCatalog.ConsoleApp.Data.Handlers.QueryHandlers
{
    public interface IGetProductsQueryHandler
    {
        IEnumerable<Product> Handle(GetProductsQuery query);
    }
}