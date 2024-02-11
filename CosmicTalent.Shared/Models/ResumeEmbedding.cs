namespace CosmicTalent.Shared.Models
{
    public class ResumeEmbedding : BaseEntity
    {
        public string Content { get; set; }
        public string EmpId { get; set; }
        public string EmpName { get; set; }
        public string Type { get; set; }
        public float[]? resumevector { get; set; }
        public int Tokens { get; set; }
    }
}
