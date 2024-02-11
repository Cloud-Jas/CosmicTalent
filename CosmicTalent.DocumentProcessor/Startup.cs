using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;
using AzureFunctions.Extensions.Middleware.Abstractions;
using AzureFunctions.Extensions.Middleware.Infrastructure;
using CosmicTalent.DocumentProcessor.Middlewares;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using StackExchange.Redis;
using CosmicTalent.DocumentProcessor.Interfaces;
using CosmicTalent.DocumentProcessor.Services;

[assembly: FunctionsStartup(typeof(CosmicTalent.DocumentProcessor.Startup))]
namespace CosmicTalent.DocumentProcessor
{
    public class Startup : FunctionsStartup
    {
        private IConfiguration Configuration { get; set; }
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddCommonConfiguration(Configuration);
            builder.Services.AddAzureAppConfiguration();
            builder.Services.AddFeatureManagement();
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(Configuration.GetValue<string>("RedisConnString")));
            builder.Services.AddSingleton<ITelemetryService, TelemetryService>(sp => new TelemetryService(sp.GetService<TelemetryConfiguration>()));
            builder.Services.AddTransient<IHttpMiddlewareBuilder, HttpMiddlewareBuilder>((serviceProvider) =>
            {
                var funcBuilder = new HttpMiddlewareBuilder(serviceProvider.GetRequiredService<IHttpContextAccessor>());
                funcBuilder.Use(new ExceptionHandlingMiddleware(serviceProvider.GetService<ILogger<ExceptionHandlingMiddleware>>(), serviceProvider.GetService<ITelemetryService>()));
                funcBuilder.Use(new CorrelationIdMiddleware(serviceProvider.GetService<ILogger<CorrelationIdMiddleware>>()));
                funcBuilder.UseWhen(ctx => ctx != null && ctx.Request.Path.ToString().StartsWith("/api/v1/"),
                    new AuthorizationMiddleware(serviceProvider.GetService<ILogger<AuthorizationMiddleware>>(), serviceProvider.GetService<IConfiguration>(), serviceProvider.GetService<ITelemetryService>()));                
                return funcBuilder;
            });
            builder.Services.AddTransient<IDocumentRecognizerService, DocumentRecognizerService>();
            builder.Services.AddSingleton<DocumentAnalysisClient>(ServiceProvider =>
            {
                string formRecognizerKey = Configuration.GetValue<string>("DocumentIntelligenceKey");
                string formRecognizerURI = Configuration.GetValue<string>("DocumentIntelligenceURI");
                return new DocumentAnalysisClient(new Uri(formRecognizerURI), new AzureKeyCredential(formRecognizerKey));
            });

        }
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            Configuration = builder.ConfigurationBuilder
               .SetBasePath(context.ApplicationRootPath)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
               .AddEnvironmentVariables().Build();
        }
    }
}
