using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Serilog;
using Serilog.Events;

namespace CosmicTalent.DocumentProcessor
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCommonConfiguration(this IServiceCollection services,IConfiguration configuration)
        {
            var seqServerUrl = configuration.GetSection("Serilog");

            string url = seqServerUrl.GetValue<string>("SeqServerUrl");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Azure.Storage.Blobs",LogEventLevel.Warning)
                .MinimumLevel.Override("Azure.Messaging.ServiceBus",LogEventLevel.Warning)
                .Enrich.WithProperty("ApplicationContext", "CosmicTalent.DocumentProcessor")
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level} {CorrelationId}] {Message}{NewLine}{Exception}")
                .WriteTo.Seq(string.IsNullOrWhiteSpace(url) ? "http://seq" : url)
                .WriteTo.ApplicationInsights(configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"], TelemetryConverter.Traces,LogEventLevel.Information)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            services.AddLogging(logging =>
            {
                logging.AddSerilog(Log.Logger, dispose: true);
            });

            services.AddApplicationInsightsTelemetry();
            services.Configure<TelemetryConfiguration>((o) => {
                o.ConnectionString = configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING");
                o.TelemetryInitializers.Add(new OperationCorrelationTelemetryInitializer());
            });
            
            services.AddSingleton<CloudBlobClient>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var storageConnectionString = configuration["StorageConnectionString"];
                var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
                return storageAccount.CreateCloudBlobClient();
            });
            services.AddSingleton<Startup>();
            
            return services;
        }        
    }
}
