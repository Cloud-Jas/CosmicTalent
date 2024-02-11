using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace CosmicTalent.DocumentProcessor
{
    public class TelemetryService : ITelemetryService
    {
        private readonly TelemetryClient client;
        public TelemetryService(TelemetryConfiguration telemetryConfiguration)
        {
            client = new TelemetryClient(telemetryConfiguration);
        }
        TelemetryClient ITelemetryService.telemetryClient => client;
    }
}