using Microsoft.ApplicationInsights;

namespace CosmicTalent.DocumentProcessor
{
    public interface ITelemetryService
    {
        public TelemetryClient telemetryClient { get; }
    }
}