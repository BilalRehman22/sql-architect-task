using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.DataFactory;
using EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos;
using Azure.ResourceManager.DataFactory.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions;
using Microsoft.Extensions.Options;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services
{
    public class EtlPipelineService : IEtlPipelineService
    {
        private readonly AzureSettings _azureSettings;
        private readonly AdfSettings _adfSettings;
        private ArmClient _armClient;

        public EtlPipelineService(IOptions<AzureSettings> azureSettings, IOptions<AdfSettings> adfSettings)
        {
            _azureSettings = azureSettings?.Value ?? throw new Exception("Azure settings not configured");
            _adfSettings = adfSettings?.Value ?? throw new Exception("Azure Data Factory settings not configured");

            _armClient = CreateArmClient();
        }

        public Result<PipelineCreateRunResult, HttpError> RunPipeline()
        {
            var dataFactoryResource = _armClient.GetDataFactoryResource(new ResourceIdentifier(_adfSettings.ResourceId));

            var pipeline = dataFactoryResource.GetDataFactoryPipeline(_adfSettings.PipelineName).Value;
            var runResponse = pipeline.CreateRun();

            return runResponse.Result();
        }

        public Result<DataFactoryPipelineRunInfo, HttpError> CheckPipelineStatus(Guid runId)
        {
            var dataFactoryResource = _armClient.GetDataFactoryResource(new ResourceIdentifier(_adfSettings.ResourceId));

            var pipelineRun = dataFactoryResource.GetPipelineRun(runId.ToString());

            return pipelineRun.Result();
        }

        private ArmClient CreateArmClient()
        {
            return new ArmClient(
                new ClientSecretCredential(
                    _azureSettings.TenantId,
                    _adfSettings.ApplicationId,
                    _adfSettings.AuthKey,
                    new TokenCredentialOptions
                    {
                        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                    }),
                _azureSettings.SubscriptionId,
                new ArmClientOptions { Environment = ArmEnvironment.AzurePublicCloud }
            );
        }
    }
}
