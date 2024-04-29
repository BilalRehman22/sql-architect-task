namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Helpers
{
    public interface IAuthHelper
    {
        Task<string> GetDatabaseAccessTokenAsync();
    }
}