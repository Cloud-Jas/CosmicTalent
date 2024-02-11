namespace CosmicTalent.Shared.Models
{
    public class CompletionResponse
    {
        public string CompletionText { get; set; }
        public int RagTokens { get; set; }
        public int CompletionTokens { get; set; }
    }
}
