using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using CosmicTalent.DocumentProcessor.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CosmicTalent.DocumentProcessor.Services
{
    public class DocumentRecognizerService : IDocumentRecognizerService
    {
        private readonly DocumentAnalysisClient _documentAnalysisClient;
        private readonly ILogger<DocumentRecognizerService> _log;
        private readonly IConfiguration _configuration;

        public DocumentRecognizerService(DocumentAnalysisClient documentAnalysisClient, ILogger<DocumentRecognizerService> log, IConfiguration configuration)
        {
            _documentAnalysisClient = documentAnalysisClient;
            _configuration = configuration;
            _log = log;
        }
        public async Task<AnalyzeResult> GetDocumentDetails(Stream content, string modelId)
        {
            try
            {
                var operation = await _documentAnalysisClient.AnalyzeDocumentAsync(WaitUntil.Completed, _configuration.GetValue<string>(modelId), content);

                var result = operation.Value;

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError("Error Message: {0} Stack Trace: {1}", ex.Message, ex.StackTrace);

                throw;
            }
        }


    }
}