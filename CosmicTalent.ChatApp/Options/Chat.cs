using CosmicTalent.ChatApp;

namespace CosmicTalent.ChatApp.Options;

public record Chat
{
    public required ILogger Logger { get; init; }
}
