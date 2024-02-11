using AzureFunctions.Extensions.Middleware.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace CosmicTalent.DocumentProcessor.Middlewares
{
    /// <summary>
    /// Adding Correlation ID to all logs
    /// </summary>
    public class CorrelationIdMiddleware : HttpMiddlewareBase
    {
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        public CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger)
        {
            _logger = logger;
        }
        public override async Task InvokeAsync(HttpContext context)
        {
            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
            {
                _logger.LogInformation($"{this.ExecutionContext.FunctionName} Correlation Midlleware triggered");
               
                await this.Next.InvokeAsync(context);
                                
            }
        }
    }
}
