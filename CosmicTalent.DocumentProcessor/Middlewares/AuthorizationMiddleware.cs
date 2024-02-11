using AzureFunctions.Extensions.Middleware.Abstractions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CosmicTalent.DocumentProcessor.Middlewares
{
    public class AuthorizationMiddleware : HttpMiddlewareBase
   {
        private readonly ILogger<AuthorizationMiddleware> _logger;
        private readonly IConfiguration _configuration;        
        private readonly TelemetryClient telemetryClient;
        public AuthorizationMiddleware(ILogger<AuthorizationMiddleware> logger,IConfiguration configuration,ITelemetryService telemetryService)
        {
            _logger = logger;
           
            _configuration = configuration;           
            
            telemetryClient = telemetryService.telemetryClient;
        }
        public override async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"{this.ExecutionContext.FunctionName} Authorization middleware triggered");

            if (!context.Request.Headers.ContainsKey("X-COSMICTALENT-ID") || !context.Request.Headers["X-COSMICTALENT-ID"].Equals(_configuration.GetValue<string>("CosmicTalentId")))
            {                
                context.Response.StatusCode = 401;

                telemetryClient.TrackEvent("DocumentProcessorFailed");

                _logger.LogInformation("DocumentProcessor authorization failed");
                
                await context.Response.WriteAsync("Authorization header is invalid or not provided");
                
                return;
            }            
            
            await this.Next.InvokeAsync(context);
        }
   }
}
