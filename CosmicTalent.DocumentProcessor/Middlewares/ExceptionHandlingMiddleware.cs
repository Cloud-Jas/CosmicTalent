using Azure.Messaging.ServiceBus.Administration;
using AzureFunctions.Extensions.Middleware.Abstractions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CosmicTalent.DocumentProcessor.Middlewares
{
    public class ExceptionHandlingMiddleware : HttpMiddlewareBase
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly TelemetryClient telemetryClient;
        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, ITelemetryService telemetryService)
        {
            _logger = logger;
            telemetryClient = telemetryService.telemetryClient;
        }
        public override async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation($"{this.ExecutionContext.FunctionName} ExceptionHandlingMiddleware triggered");

            try
            {
                await this.Next.InvokeAsync(context);
                
                stopwatch.Stop();
                
                telemetryClient.TrackMetric("ResponseTime", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                telemetryClient.TrackEvent("DocumentProcessorUnhandledException");

                _logger.LogInformation("DocumentProcessor unexpected failure");

                _logger.LogError($"Exception message: {ex.Message} Exception stack trace: {ex.StackTrace}");
                
                stopwatch.Stop();
                
                telemetryClient.TrackMetric("ResponseTime", stopwatch.ElapsedMilliseconds);

                context.Response.StatusCode = 500;

                await context.Response.WriteAsync("Unexpected error occured. Please reach out to Edenred team");
            }
        }
    }
}
