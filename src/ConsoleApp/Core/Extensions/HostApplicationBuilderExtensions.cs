using EmpowerId.ProductCatalog.ConsoleApp.BackgroundServices;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Helpers;
using EmpowerId.ProductCatalog.ConsoleApp.Core.Settings;
using EmpowerId.ProductCatalog.ConsoleApp.Data.Handlers.QueryHandlers;
using EmpowerId.ProductCatalog.ConsoleApp.Services;
using EmpowerId.ProductCatalog.ConsoleApp.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EmpowerId.ProductCatalog.ConsoleApp.Core.Extensions
{
    public static class HostApplicationBuilderExtensions
    {
        public static HostApplicationBuilder AddConfiguration(this HostApplicationBuilder builder)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = configBuilder.Build();
            var services = builder.Services;

            services.AddScoped(sp => config);
            services.Configure<AppSettings>(config.GetSection("Application"));
            services.Configure<AdfSettings>(config.GetSection("DataFactory"));
            services.Configure<AzureSettings>(config.GetSection("Azure"));
            services.Configure<CognitiveSearchSettings>(config.GetSection("CognitiveSearch"));
            services.Configure<DatabaseSettings>(config.GetSection("Database"));

            return builder;
        }

        public static HostApplicationBuilder AddServices(this HostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IGetProductsQueryHandler, GetProductsQueryHandler>();
            builder.Services.AddSingleton<IAuthHelper, AuthHelper>();

            // Register all views
            builder.Services.AddScoped<CognitiveSearchView>();
            builder.Services.AddScoped<CognitiveSearchIndexerView>();
            builder.Services.AddScoped<EtlPipelineRunnerView>();
            builder.Services.AddScoped<ProductAttributeSearchView>();
            builder.Services.AddScoped<MainMenuView>();

            // Register services
            builder.Services.AddScoped<IProductAttributeSearchService, ProductAttributeSearchService>();
            builder.Services.AddScoped<ICognitiveSearchService, CognitiveSearchService>();
            builder.Services.AddScoped<IEtlPipelineService, EtlPipelineService>();

            // Register background service
            builder.Services.AddHostedService<ChangeDataCaptureBackgroundService>();

            return builder;
        }

        public static HostApplicationBuilder AddLogging(this HostApplicationBuilder builder)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                //.WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder => 
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog(logger);
                });

            return builder;
        }
    }
}
