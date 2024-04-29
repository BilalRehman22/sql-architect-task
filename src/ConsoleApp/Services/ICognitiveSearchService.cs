using Azure.Search.Documents.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services
{
    public interface ICognitiveSearchService
    {
        Result<bool, HttpError> RunIndexer();
        Result<SearchResults<CognitiveSearchResult>, HttpError> Search(string searchTerm);
    }
}