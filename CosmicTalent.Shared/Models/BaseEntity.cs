using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CosmicTalent.Shared.Models
{
    public class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
