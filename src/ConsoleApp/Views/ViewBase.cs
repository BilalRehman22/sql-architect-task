using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace EmpowerId.ProductCatalog.ConsoleApp.Views
{
    public abstract class ViewBase
    {
        protected ILogger<ViewBase> Logger { get; }

        public ViewBase(ILogger<ViewBase> logger)
        {
            Logger = logger;
        }
        public void Render()
        {
            Console.Clear();
            OnRender();
            
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
        }

        protected abstract void OnRender();
    }
}
