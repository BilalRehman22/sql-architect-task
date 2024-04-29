using Spectre.Console;
using EmpowerId.ProductCatalog.ConsoleApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;

namespace EmpowerId.ProductCatalog.ConsoleApp.Views
{
    public class CognitiveSearchIndexerView : ViewBase
    {
        private readonly ICognitiveSearchService _cognitiveSearchService;
        private readonly CognitiveSearchSettings _congnitiveSearchSettings;

        public CognitiveSearchIndexerView(
            ICognitiveSearchService cognitiveSearchService, 
            IOptions<CognitiveSearchSettings> congnitiveSearchSettings,
            ILogger<CognitiveSearchIndexerView> logger)
            : base(logger)
        {
            _cognitiveSearchService = cognitiveSearchService;
            _congnitiveSearchSettings = congnitiveSearchSettings?.Value ?? throw new Exception("Cognitive Search settings not configured"); ;
        }

        protected override void OnRender()
        {
            AnsiConsole.WriteLine();
            var confirm = AnsiConsole.Confirm("Are you sure you'd like to run the cognitive search indexer?");

            if (!confirm)
            {
                return;
            }

            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("green"))
                .Start($"Running {_congnitiveSearchSettings.IndexerName} on Azure Cognitive Search...", ctx =>
                {
                    var result = _cognitiveSearchService.RunIndexer();
                    if (!result.IsSuccess())
                    {
                        AnsiConsole.MarkupLine($"[grey][[{DateTime.Now:hh:mm:ss tt}]][/] [red]An error occured while attempting to run the indexer. Please check application logs for details.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[grey][[{DateTime.Now:hh:mm:ss tt}]][/] [green]Indexer run completed successfuly[/]");
                    }
                });
        }
    }
}
