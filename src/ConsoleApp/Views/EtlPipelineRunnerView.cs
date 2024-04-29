using Azure.ResourceManager.DataFactory.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using EmpowerId.ProductCatalog.ConsoleApp.Services;
using EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace EmpowerId.ProductCatalog.ConsoleApp.Views
{
    public class EtlPipelineRunnerView : ViewBase
    {
        private const string PipelineSuccessStatus = "Succeeded";
        private readonly string[] PipelineProcessingStatuses = ["InProgress", "Queued"];
        private readonly AdfSettings _adfSettings;
        private readonly IEtlPipelineService _etlPipelineService;
        
        public EtlPipelineRunnerView(
            IOptions<AdfSettings> adfSettings,
            IEtlPipelineService etlPipelineService,
            ILogger<EtlPipelineRunnerView> logger) 
            : base(logger)
        {
            _adfSettings = adfSettings?.Value ?? throw new Exception("Azure Data Factory settings not configured");
            _etlPipelineService = etlPipelineService;
        }

        protected override void OnRender()
        {
            AnsiConsole.WriteLine();
            var confirm = AnsiConsole.Confirm("Are you sure you'd like to run the ETL pipeline?");

            if (!confirm)
            {
                return;
            }

            AnsiConsole.MarkupLine($"[red]Running data factory pipeline ({_adfSettings.PipelineName}) ...[/]");
            var pipelineRunResult = _etlPipelineService.RunPipeline();

            if (!pipelineRunResult.IsSuccess())
            {
                AnsiConsole.MarkupLine("[red]An error occurred while attempting to run the pipeline. Error: {0}[/]", (pipelineRunResult.Error?.Message ?? "").EscapeMarkup());
                AnsiConsole.MarkupLine("[red]Please check application logs for more details.");
                return;
            }

            AnsiConsole.MarkupLine("[green]Pipeline run was successful..[/]");
            AnsiConsole.WriteLine();

            int attempt = 0;

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("green"))
                .Start("Monitoring pipeline run status...", ctx =>
                {
                    Result<DataFactoryPipelineRunInfo, HttpError> statusCheckResult = null;

                    while (attempt <= 3)
                    {
                         statusCheckResult = _etlPipelineService.CheckPipelineStatus(pipelineRunResult.Data!.RunId);

                        if (!statusCheckResult.IsSuccess())
                        {
                            AnsiConsole.MarkupLine("[red]Failed to retrieve pipeline run status. Error: {0}[/]", (pipelineRunResult.Error?.Message ?? "").EscapeMarkup());
                            attempt++;
                            continue;
                        }

                        if (PipelineProcessingStatuses.Contains(statusCheckResult.Data!.Status))
                        {
                            AnsiConsole.MarkupLine($"[grey][[{DateTime.Now:hh:mm:ss tt}]][/] Pipeline status: {statusCheckResult.Data!.Status}");
                            Thread.Sleep(15000);
                        }
                        else
                            break;
                    }
                    
                    if (statusCheckResult?.IsSuccess() == true)
                    {
                        if (statusCheckResult.Data!.Status == PipelineSuccessStatus)
                        {
                            AnsiConsole.MarkupLine($"[grey][[{DateTime.Now:hh:mm:ss tt}]][/] [green]Pipeline run completed successfuly![/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine($"[grey][[{DateTime.Now:hh:mm:ss tt}]][/] [red]Pipeline run ended with status '{statusCheckResult.Data!.Status}'. Please try again later.[/]");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Pipeline run status could not be checked due to errors. Please check application logs for more details.");
                    }
                });
        }
    }
}
