using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Collections.Generic;

namespace CosmicTalent.DocumentProcessor.Interfaces
{
    public interface IDocumentRecognizerService
    {
        public Task<AnalyzeResult> GetDocumentDetails(Stream content, string modelId);
    }
}
