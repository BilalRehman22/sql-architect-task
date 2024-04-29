using Azure.Search.Documents.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using EmpowerId.ProductCatalog.ConsoleApp.Services;
using EmpowerId.ProductCatalog.ConsoleApp.Services.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace EmpowerId.ProductCatalog.ConsoleApp.Views
{
    public class CognitiveSearchView : ViewBase
    {
        private readonly ICognitiveSearchService _cognitiveSearchService;
        private readonly AppSettings _appSettings;

        public CognitiveSearchView(
            IOptions<AppSettings> appSettings, 
            ICognitiveSearchService cognitiveSearchService, 
            ILogger<CognitiveSearchView> logger)
            : base(logger)
        {
            _appSettings = appSettings?.Value ?? throw new Exception("App settings not configured");
            _cognitiveSearchService = cognitiveSearchService;
        }

        protected override void OnRender()
        {
            AnsiConsole.WriteLine();
            var searchString = AnsiConsole.Ask<string>("What would you like to search today? (e.g. beauty, cake, coconut milk, sesame oil)");

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                AnsiConsole.MarkupLine("[red]Searching ...[/]");
                var result = _cognitiveSearchService.Search(searchString.Trim());

                if (!result.IsSuccess())
                {
                    AnsiConsole.MarkupLine($"[grey][[{DateTime.Now:hh:mm:ss tt}]][/] [red]An error occured while attempting to search. Please check application logs for details.[/]");
                }
                else
                {
                    RenderSearchResults(searchString, result.Data);
                }
            }
        }
        private void RenderSearchResults(string searchTerm, SearchResults<CognitiveSearchResult>? searchResults)
        {
            AnsiConsole.WriteLine();

            if (searchResults == null || searchResults.TotalCount == 0)
            {
                AnsiConsole.MarkupInterpolated($"No search results found for: [bold yellow on blue]{searchTerm}[/]");
                return;
            }
            
            AnsiConsole.MarkupInterpolated($"Showing 1-{Math.Min(_appSettings.SearchPageSize, searchResults.TotalCount!.Value)} of {searchResults.TotalCount} results for: [bold yellow on blue]{searchTerm}[/]");
            AnsiConsole.WriteLine();

            var table = new Table();
            table.BorderColor(Color.Green4);
            table.RoundedBorder();

            // Add some columns
            table.AddColumn("id");
            table.AddColumn("product_name");
            table.AddColumn("category");
            table.AddColumn("description");

            // Add some rows
            foreach (var result in searchResults.GetResults())
            {
                table.AddRow(
                    Markup.Escape(result.Document.ProductId ?? ""),
                    Markup.Escape(result.Document.ProductName ?? ""),
                    Markup.Escape(result.Document.CategoryName ?? ""),
                    Markup.Escape(result.Document.Description ?? "")
                );
            }

            // Render the table to the console
            AnsiConsole.Write(table);
        }

    }
}
