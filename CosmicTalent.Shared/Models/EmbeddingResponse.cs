namespace CosmicTalent.Shared.Models
{
    public class EmbeddingResponse
    {
        public int PromptTokens { get; set; }
        public float[] PromptVectors { get; set; }
    }
}
