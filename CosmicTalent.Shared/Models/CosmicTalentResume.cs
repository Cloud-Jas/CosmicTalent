using Azure.AI.FormRecognizer.DocumentAnalysis;

namespace CosmicTalent.Shared.Models
{
    public class CosmicTalentResume
    {        
        public string? BlobUri { get; set; }
        public string? EmployeeId { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }    
        public string? Content { get; set; }
        public List<CustomAnalyzedDocument> Documents { get; set; }
        public List<CustomTable> Tables { get; set; } = default;
    }
}
