using Azure.AI.OpenAI;
using CosmicTalent.ChatApp.Constants;
using CosmicTalent.Shared.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SharpToken;
using System.Net;

namespace CosmicTalent.ChatApp;

public class ChatApiClient
{
    private static List<Session>? _sessions = null;
    private readonly HttpClient _chatClient;
    private readonly HttpClient _openAIClient;
    private readonly int _maxTokens;    

    public ChatApiClient(IHttpClientFactory httpClientFactory,IConfiguration configuration)
    {
        _chatClient = httpClientFactory.CreateClient("MongoDbClient");
        _openAIClient = httpClientFactory.CreateClient("OpenAIClient");
        _maxTokens = configuration.GetValue<int>("OpenAI:Prompt:MaxTokens");        
    }

    public async Task<List<Session>?> GetAllChatSessionsAsync()
    {
        _sessions = await _chatClient.GetFromJsonAsync<List<Session>>("sessions");
        return _sessions;
    }

    public async Task<List<Message>?> GetChatSessionMessagesAsync(string? sessionId)
    {

        List<Message>? messages = new();

        if (_sessions?.Count == 0)
        {
            return Enumerable.Empty<Message>().ToList();
        }

        if (_sessions != null)
        {

            int index = _sessions.FindIndex(s => s.SessionId == sessionId);

            if (_sessions[index].Messages?.Count == 0)
            {
                messages = await _chatClient.GetFromJsonAsync<List<Message>>($"messages/{sessionId}");
                _sessions[index].Messages = messages;
            }
            else
            {
                messages = _sessions[index].Messages;
            }
        }

        return messages;
    }

    public async Task CreateNewChatSessionAsync()
    {
        Session session = new();
        _sessions?.Add(session);
        await _chatClient.PostAsJsonAsync("sessions", session);
    }

    public async Task RenameChatSessionAsync(string? sessionId, string newChatSessionName)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        if (_sessions != null)
        {
            int index = _sessions.FindIndex(s => s.SessionId == sessionId);
            _sessions[index].Name = newChatSessionName;
            await _chatClient.PutAsJsonAsync($"sessions/{sessionId}", _sessions[index]);            
        }
    }

    public async Task DeleteChatSessionAsync(string? sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        if (_sessions != null)
        {
            int index = _sessions.FindIndex(s => s.SessionId == sessionId);
            _sessions.RemoveAt(index);
            await _chatClient.DeleteAsync($"sessions/{sessionId}");            
        }
    }

    public async Task<string> GetChatCompletionAsync(string? sessionId, string userPrompt)
    {
        try            
        {
            ArgumentNullException.ThrowIfNull(sessionId);
            string encodedUserPrompt = WebUtility.UrlEncode(userPrompt);
            EmbeddingResponse embeddingResponse = await _openAIClient.GetFromJsonAsync<EmbeddingResponse>($"embeddings/{encodedUserPrompt}/sessions/{sessionId}");
            if (embeddingResponse != null)
            {
                Message promptMessage = new Message(sessionId, nameof(Participants.User), embeddingResponse.PromptTokens, default, userPrompt);
                string retrievedDocuments = await (await _chatClient.PostAsJsonAsync<float[]>($"employee/vectorSearch", embeddingResponse.PromptVectors)).Content.ReadAsStringAsync();
                string conversation = GetConversationHistory(sessionId);
                (string augmentedContent, string conversationAndUserPrompt) = BuildPrompts(userPrompt, conversation, retrievedDocuments);
                var responseBody = await (await _openAIClient.PostAsJsonAsync($"chatCompletions", new CompletionRequest { SessionId = sessionId, Conversation = conversationAndUserPrompt, AugmentedContent = augmentedContent })).Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<CompletionResponse>(responseBody);
                if (response != null)
                {
                    Message completionMessage = new Message(sessionId, nameof(Participants.Assistant), response.CompletionTokens, response.RagTokens, response.CompletionText);
                    await AddPromptCompletionMessagesAsync(sessionId, promptMessage, completionMessage);
                    return response.CompletionText;
                }
                else
                {
                    return string.Empty;
                }
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            string message = $"ChatService.GetChatCompletionAsync(): {ex.Message}";            
            throw;
        }
    }

    private (string augmentedContent, string conversationAndUserPrompt) BuildPrompts(string userPrompt, string conversation, string retrievedData)
    {

        string updatedAugmentedContent = "";
        string updatedConversationAndUserPrompt = "";        
        int bufferTokens = 200;
        var encoding = GptEncoding.GetEncoding("cl100k_base");        
        List<int> ragVectors = encoding.Encode(retrievedData);
        int ragTokens = ragVectors.Count;
        List<int> convVectors = encoding.Encode(conversation);
        int convTokens = convVectors.Count;
        int userPromptTokens = encoding.Encode(userPrompt).Count;        
        int totalTokens = ragTokens + convTokens + userPromptTokens + bufferTokens;        
        if (totalTokens > _maxTokens)
        {            
            int tokensToReduce = totalTokens - _maxTokens;            
            float ragTokenPct = (float)ragTokens / totalTokens;
            float conTokenPct = (float)convTokens / totalTokens;            
            int newRagTokens = (int)Math.Round(ragTokens - (ragTokenPct * tokensToReduce), 0);
            int newConvTokens = (int)Math.Round(convTokens - (conTokenPct * tokensToReduce), 0);            
            List<int> trimmedRagVectors = ragVectors.GetRange(0, newRagTokens);            
            updatedAugmentedContent = encoding.Decode(trimmedRagVectors);
            int offset = convVectors.Count - newConvTokens;
            List<int> trimmedConvVectors = convVectors.GetRange(offset, newConvTokens);
            updatedConversationAndUserPrompt = encoding.Decode(trimmedConvVectors);
            updatedConversationAndUserPrompt += Environment.NewLine + userPrompt;
        }        
        else
        {            
            updatedAugmentedContent = retrievedData;
            updatedConversationAndUserPrompt = conversation + Environment.NewLine + userPrompt;
        }


        return (augmentedContent: updatedAugmentedContent, conversationAndUserPrompt: updatedConversationAndUserPrompt);

    }
    private string GetConversationHistory(string sessionId)
    {
        int? tokensUsed = 0;
        if (_sessions != null)
        {
            int index = _sessions.FindIndex(s => s.SessionId == sessionId);
            List<Message>? conversationMessages = _sessions[index].Messages?.ToList();
            var trimmedMessages = conversationMessages
                .OrderByDescending(m => m.TimeStamp)
                .TakeWhile(m => (tokensUsed += m.Tokens) <= _maxTokens)
                .Select(m => m.Text)
                .ToList();

            trimmedMessages.Reverse();
            string conversation = string.Join(Environment.NewLine, trimmedMessages.ToArray());
            return conversation;
        }

        return string.Empty;

    }
    public async Task<string> SummarizeChatSessionNameAsync(string? sessionId, string prompt)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        var response = await _openAIClient.PostAsJsonAsync("summarize", new SummarizeRequest { SessionId= sessionId, Prompt = prompt });
        var summary = await response.Content.ReadAsStringAsync();
        await RenameChatSessionAsync(sessionId, summary);
        return summary;
    }

    private async Task AddPromptMessageAsync(string sessionId, string promptText)
    {
        if (_sessions != null)
        {
            Message promptMessage = new(sessionId, nameof(Participants.User), default, default, promptText);
            int index = _sessions.FindIndex(s => s.SessionId == sessionId);
            _sessions[index].AddMessage(promptMessage);
            await _chatClient.PostAsJsonAsync($"sessions/{sessionId}/messages", promptMessage);            
        }
    }

    private async Task AddPromptCompletionMessagesAsync(string sessionId, Message promptMessage, Message completionMessage)
    {
        if (_sessions != null)
        {
            int index = _sessions.FindIndex(s => s.SessionId == sessionId);
            _sessions[index].AddMessage(promptMessage);
            _sessions[index].AddMessage(completionMessage);
            _sessions[index].TokensUsed += promptMessage.Tokens;
            _sessions[index].TokensUsed += completionMessage.PromptTokens;
            _sessions[index].TokensUsed += completionMessage.Tokens;
            await _chatClient.PostAsJsonAsync($"sessions/{sessionId}/messages", promptMessage);            
            await _chatClient.PostAsJsonAsync($"sessions/{sessionId}/messages", completionMessage);            
        }
    }
}
