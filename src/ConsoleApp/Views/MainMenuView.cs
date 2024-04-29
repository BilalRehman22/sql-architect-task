using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace EmpowerId.ProductCatalog.ConsoleApp.Views
{
    public class MainMenuView
    {
        private static readonly FigletFont FigletFont = FigletFont.Load("Assets/starwars.flf");
        private static readonly string[] MainMenuOptions = [
        "Initiate & monitor ETL pipeline",
            "Re-run Cognitive Search indexer",
            "Search using Azure Cognitive Search",
            "Search product attributes",
            "Quit"
        ];

        private readonly IServiceProvider _serviceProvider;
        private bool _startUp;

        public MainMenuView(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _startUp = true;
        }

        public void Render()
        {
            do
            {
                var selectedOption = RenderMainMenu();

                ViewBase? activeView;

                switch (selectedOption)
                {
                    case 0:
                        activeView = _serviceProvider.
                            GetRequiredService<EtlPipelineRunnerView>();
                        break;
                    case 1:
                        activeView = _serviceProvider.
                            GetRequiredService<CognitiveSearchIndexerView>();
                        break;
                    case 2:
                        activeView = _serviceProvider.
                            GetRequiredService<CognitiveSearchView>();
                        break;
                    case 3:
                        activeView = _serviceProvider.
                            GetRequiredService<ProductAttributeSearchView>();
                        break;
                    default:
                        Console.WriteLine("Press any key to exit..");
                        Console.ReadKey();
                        return;
                }

                if (activeView != null)
                {
                    activeView.Render();
                }
            } while (true);
        }

        private int RenderMainMenu()
        {
            Console.Clear();
            AnsiConsole.Write(new FigletText(FigletFont, "WELCOME"));
            AnsiConsole.WriteLine();

            if (_startUp)
            {
                RenderStartupNotes();
                _startUp = false;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow on blue] Main Menu [/]");
            AnsiConsole.WriteLine();
            var menu = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Please choose an option from below:[/]")
                    .AddChoices(MainMenuOptions));

            var selectedOption = Array.FindIndex(MainMenuOptions, s => s == menu);
            return selectedOption;
        }

        private static void RenderStartupNotes()
        {
            var panel = new Panel(
                                new Rows(
                                    new Text("* Change Data Capture (CDC) running as background service every 5 minutes."),
                                    new Markup("* Application log file location: '[olive]logs\\log<date>.txt[/]'.")
                                )
                            );

            panel.Header = new PanelHeader("Note: ", Justify.Left);

            AnsiConsole.Write(panel);
        }
    }
}
