using Azure.AI.OpenAI;
using CosmicTalent.Shared.Models;

namespace CosmicTalent.Shared.Interfaces
{
    public interface IOpenAIService
    {
        Task<CompletionResponse> GetChatCompletionAsync(string sessionId, string userPrompt, string documents);
        Task<(float[],int)> GetEmbeddingsAsync(string input,string? sessionId =default);
        public Task<string> SummarizeAsync(string? sessionId, string userPrompt);
    }
}