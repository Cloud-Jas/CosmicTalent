using Azure;
using Azure.AI.OpenAI;
using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace CosmicTalent.Shared.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly ILogger<OpenAIService> _logger;
        private readonly OpenAIClient _openAiClient;
        private readonly string _embeddingDeploymentId;
        private readonly string _deploymentId;
        private readonly int _maxTokens;
        private readonly float _temperature;
        private readonly string _systemPromptTalentAssistant = @"
        You are an HR assistant for the Flyers Soft company. 
        You are designed to provide helpful answers to manager questions or general queries about employee mentioned below.
        If you are provided with Job description (JD) and asked to look for relevant fit for the profile, then make sure to pick relevant 
        employee even if there is no information provided about any employee's experience or qualifications from the information mentioned below

        Instructions:
        - Only answer questions related to the information provided below,        
        - If you're unsure of an answer, you can say ""I don't know"" or ""I'm not sure"" and recommend users search themselves.

        Text of relevant information:";

        private readonly string _summarizePrompt = @"
        Summarize the text below in one or two words to use as a label in a button on a web page. Output words only. Summarize the text below here:" + Environment.NewLine;
        private readonly string _summarizeProjectPrompt = @"
        Summarize the Project details below without missing any contexts. Capture all details necessary for HR assistant to validate on a profile. Summarize the text below here:" + Environment.NewLine;
        public OpenAIService(ILogger<OpenAIService> logger, OpenAIClient openAIClient, IConfiguration configuration)
        {
            _logger = logger;
            _openAiClient = openAIClient;
            _deploymentId = configuration.GetValue<string>("OpenAI:DeploymentId") ?? throw new ArgumentNullException(_deploymentId);
            _embeddingDeploymentId = configuration.GetValue<string>("OpenAI:EmbeddingDeploymentId") ?? throw new ArgumentNullException(_embeddingDeploymentId);
            _maxTokens = configuration.GetValue<int>("OpenAI:Prompt:MaxTokens");
            _temperature = configuration.GetValue<float>("OpenAI:Prompt:Temperature");
        }
        public async Task<(float[], int)> GetEmbeddingsAsync(string input, string? sessionId = default)
        {
            try
            {
                EmbeddingsOptions options = new EmbeddingsOptions(_embeddingDeploymentId, new List<string> { input });

                if (!string.IsNullOrEmpty(sessionId))
                    options.User = sessionId;

                var response = await _openAiClient.GetEmbeddingsAsync(options);
                Embeddings embeddingValue = response.Value;
                var token = embeddingValue.Usage.TotalTokens;
                var embeddings = embeddingValue.Data[0].Embedding.ToArray();

                return (embeddings, token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception message : {ex.Message} Stack trace: {ex.StackTrace}");
                throw;

            }
        }
        public async Task<string> SummarizeAsync(string? sessionId, string userPrompt)
        {

            var systemMessage = new ChatRequestSystemMessage(sessionId != null ? _summarizePrompt : _summarizeProjectPrompt);
            var userMessage = new ChatRequestUserMessage(userPrompt);

            ChatCompletionsOptions options = new()
            {
                DeploymentName = _deploymentId,
                Messages = {
                systemMessage,
                userMessage
            },
                User = sessionId ?? "Summarizer",
                MaxTokens = 200,
                Temperature = 0.0f,
                NucleusSamplingFactor = 1.0f,
                FrequencyPenalty = 0,
                PresencePenalty = 0
            };

            Response<ChatCompletions> completionsResponse = await _openAiClient.GetChatCompletionsAsync(options);

            ChatCompletions completions = completionsResponse.Value;
            string output = completions.Choices[0].Message.Content;
            string summary = Regex.Replace(output, @"[^a-zA-Z0-9\s]", "");

            return summary;
        }
        public async Task<CompletionResponse> GetChatCompletionAsync(string sessionId, string userPrompt, string documents)
        {
            try
            {
                var systemMessage = new ChatRequestSystemMessage(_systemPromptTalentAssistant + documents);
                var userMessage = new ChatRequestUserMessage(userPrompt);

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    DeploymentName = _deploymentId,
                    Messages =
                    {
                        systemMessage,
                        userMessage
                    },
                    User = sessionId,
                    MaxTokens = _maxTokens,
                    Temperature = _temperature,
                };

                var result = await _openAiClient
                                       .GetChatCompletionsAsync(chatCompletionsOptions)
                                       .ConfigureAwait(false);
                var completions = result.Value;

                return new CompletionResponse
                {
                    CompletionText = completions.Choices[0].Message.Content,
                    RagTokens = completions.Usage.PromptTokens,
                    CompletionTokens = completions.Usage.CompletionTokens
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception message : {ex.Message} Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
