using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CosmicTalent.Shared.Models
{
    public class Employee : BaseEntity
    {        
        public required string EmpId { get; set; }  
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsOccupied { get; set; }
        public string? Content { get; set; }
        public string? ResumeUri { get; set; }
        public float[]? ResumeVector { get; set; } = null;
        public float[]? resumevector { get; set; } = null;
    }
}
