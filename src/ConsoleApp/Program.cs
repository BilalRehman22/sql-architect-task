using EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions;
using EmpowerId.ProductCatalog.ConsoleApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = Host.CreateApplicationBuilder(args)
    .AddConfiguration()
    .AddServices()
    .AddLogging()
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting...");

await host.StartAsync();

try
{
    var mainMenu = host.Services.GetRequiredService<MainMenuView>();
    mainMenu.Render();
}
catch (Exception ex)
{
    logger.LogError(ex, ex.Message);
    Console.WriteLine("Press any key to exit..");
    Console.ReadKey();
}
