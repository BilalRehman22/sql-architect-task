using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Helpers
{
    public class AuthHelper : IAuthHelper
    {
        private readonly DatabaseSettings _dbSettings;
        private readonly AzureSettings _azureSettings;

        private static IConfidentialClientApplication? SingletonClientApp { get; set; }

        public AuthHelper(IOptions<DatabaseSettings> dbSettings, IOptions<AzureSettings> azureSettings)
        {
            _dbSettings = dbSettings?.Value ?? throw new Exception("Database settings not configured");
            _azureSettings = azureSettings?.Value ?? throw new Exception("Azure settings not configured");
        }

        public async Task<string> GetDatabaseAccessTokenAsync()
        {
            if (SingletonClientApp == null)
            {
                SingletonClientApp = ConfidentialClientApplicationBuilder
                    .Create(_dbSettings.ClientId)
                    .WithAuthority(AzureCloudInstance.AzurePublic, _azureSettings.TenantId)
                    .WithClientSecret(_dbSettings.ClientSecret)
                    .Build();
            }

            var authResult = await SingletonClientApp.AcquireTokenForClient(scopes: new[] { $"{_dbSettings.ResourceId}/.default" })
                .ExecuteAsync();

            return authResult.AccessToken;
        }
    }
}
