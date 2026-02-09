using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GamerLFG.Models
{
    public class Endorsement
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public string EndorsementType { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string? LobbyId { get; set; } // Optional: Link to specific session
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
