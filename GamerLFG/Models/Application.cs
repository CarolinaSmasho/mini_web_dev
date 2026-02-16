using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GamerLFG.Models
{
    public class Application
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? LobbyId { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
        
        public string? Status { get; set; } // เช่น "Pending", "Accepted", "Rejected"
        public string? AppliedRole { get; set; }
        
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}