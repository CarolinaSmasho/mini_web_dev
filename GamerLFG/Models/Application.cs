using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GamerLFG.Models
{
    public class Application
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        
        public string LobbyId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> DesiredRoles { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Cancelled
        public int YesVotes { get; set; }
        public int NoVotes { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}
