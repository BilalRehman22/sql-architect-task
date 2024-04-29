using Azure.ResourceManager.DataFactory.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos;

namespace EmpowerId.ProductCatalog.ConsoleApp.Services
{
    public interface IEtlPipelineService
    {
        Result<DataFactoryPipelineRunInfo, HttpError> CheckPipelineStatus(Guid runId);
        Result<PipelineCreateRunResult, HttpError> RunPipeline();
    }
}