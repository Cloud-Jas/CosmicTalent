﻿using CosmicTalent.Shared.Interfaces;
using CosmicTalent.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CosmicTalent.TalentProcessor.Controllers
{
    [ApiController]
    [Route("api/openai")]
    public class OpenAIController : Controller
    {
        private readonly IOpenAIService _openAIservice;

        public OpenAIController(IOpenAIService openAIService)
        {
            _openAIservice = openAIService;
        }

        [HttpPost("embeddings/sessions/{sessionId}")]
        public async Task<ActionResult<EmbeddingResponse>> GetEmbeddingAsync(UserPromptRequest userPrompt, string sessionId)
        {
            try
            {                
                var (embeddings, tokens) = await _openAIservice.GetEmbeddingsAsync(userPrompt.Prompt, sessionId);
                return Ok(new EmbeddingResponse
                {
                    PromptTokens = tokens,
                    PromptVectors = embeddings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("chatCompletions")]
        public async Task<ActionResult<CompletionResponse>> GetChatCompletionsAsync(CompletionRequest request)
        {
            try
            {
                var response = await _openAIservice.GetChatCompletionAsync(request.SessionId, request.Conversation, request.AugmentedContent);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("summarize")]
        public async Task<ActionResult<string>> SummarizePromptAsync(SummarizeRequest summarizeRequest)
        {
            try
            {
                var response = await _openAIservice.SummarizeAsync(summarizeRequest.SessionId, summarizeRequest.Prompt);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }        
    }
}
