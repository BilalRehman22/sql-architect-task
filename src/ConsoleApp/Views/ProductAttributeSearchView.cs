using EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using EmpowerId.ProductCatalog.ConsoleApp.Data.Models;
using EmpowerId.ProductCatalog.ConsoleApp.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace EmpowerId.ProductCatalog.ConsoleApp.Views
{
    public class ProductAttributeSearchView : ViewBase
    {
        private readonly AppSettings _appSettings;
        private readonly IProductAttributeSearchService _productAttributeSearchService;

        public ProductAttributeSearchView(
            IOptions<AppSettings> appSettings,
            IProductAttributeSearchService productAttributeSearchService, 
            ILogger<ProductAttributeSearchView> logger) 
            : base(logger)
        {
            _appSettings = appSettings?.Value ?? throw new Exception("App settings not configured");
            _productAttributeSearchService = productAttributeSearchService;
        }

        protected override void OnRender()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]Search products by attributes. Enter [grey](blank)[/] to exlude attribute.[/]\n");

            var console = AnsiConsole.Console;
            var productId = console.PromptOptional<long?>(
                prompt: "Product ID [grey](equals)[/]: ",
                validationErrorMessage: "[red]Product ID must be a number.[/]",
                validator: id =>
                {
                    return id switch
                    {
                        <= 0 => ValidationResult.Error("[red]Product ID must be greater than 0[/]"),
                        _ => ValidationResult.Success(),
                    };
                });

            var productName = console.PromptOptional<string?>("Product Name [grey](contains)[/]: ");
            var categoryName = console.PromptOptional<string?>("Category Name [grey](contains)[/]: ");
            var description = console.PromptOptional<string?>("Description [grey](contains)[/]: ");

            var priceMin = console.PromptOptional<decimal?>(
                prompt: "Minimum Price [grey](greater than or equals to)[/]: $",
                validationErrorMessage: "[red]Minimum Price must be a number.[/]",
                validator: price =>
                {
                    return price switch
                    {
                        < 0 => ValidationResult.Error("[red]Minimum Price value must be greater than or equal to 0[/]"),
                        _ => ValidationResult.Success(),
                    };
                });

            var priceMax = console.PromptOptional<decimal?>(
                prompt: "Maximum Price [grey](less than or equals to)[/]: $",
                validationErrorMessage: "[red]Maximum Price must be a number.[/]",
                validator: price =>
                {
                    return price switch
                    {
                        < 0 => ValidationResult.Error("[red]Maximum Price value must be greater than or equal to 0[/]"),
                        _ => priceMin == null || price > priceMin ?
                            ValidationResult.Success() :
                            ValidationResult.Error("[red]Maximum Price value must be greater than or equal to the minimum price[/]"),
                    };
                });

            var dateAdded = console.PromptOptional<DateTime?>(
                prompt: "Date added [[dd/MM/yyyy]] [grey](equals)[/]: ",
                validationErrorMessage: "Invalid date format. Please enter the date in the format [yellow][[dd/MM]yyyy]][/].");

            var imageUrl = console.PromptOptional<string?>("Image Url [grey](contains)[/]: ");

            AnsiConsole.MarkupLine("[red]Searching ...[/]");

            var searchResults = _productAttributeSearchService.Search(productId, productName, categoryName, 
                description, priceMin, priceMax, imageUrl, dateAdded);

            RenderProductSearchResults(searchResults?.ToList());
        }

        private void RenderProductSearchResults(List<Product>? searchResults)
        {
            AnsiConsole.WriteLine();

            if (searchResults == null || !searchResults.Any())
            {
                AnsiConsole.MarkupLine("No results found against the searched attributes");
            }

            AnsiConsole.MarkupLine($"Showing top {_appSettings.SearchPageSize} results for the searched attributes");
            AnsiConsole.WriteLine();

            var table = new Table();
            table.BorderColor(Color.Green4);
            table.RoundedBorder();

            // Add some columns
            table.AddColumn("id");
            table.AddColumn("product_name");
            table.AddColumn("category");
            table.AddColumn("price");
            table.AddColumn("description");
            table.AddColumn("image_url");
            table.AddColumn("date_added");

            // Add some rows
            foreach (var product in searchResults!)
            {
                table.AddRow(
                    product.ProductId.ToString(),
                    Markup.Escape(product.ProductName ?? ""),
                    Markup.Escape(product.CategoryName ?? ""),
                    Markup.Escape(product.Price?.ToString("c") ?? ""),
                    Markup.Escape(product.Description ?? ""),
                    Markup.Escape(product.ImageUrl ?? ""),
                    Markup.Escape(product.DateAdded.ToString("dd/MM/yyyyy"))
                );
            }

            // Render the table to the console
            AnsiConsole.Write(table);
        }
    }
}
