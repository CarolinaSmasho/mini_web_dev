using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GamerLFG.Models
{
    public class Vote
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        
        public string ApplicationId { get; set; } = string.Empty;
        public string VoterId { get; set; } = string.Empty;
        public string VoteType { get; set; } = string.Empty; // yes, no
        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
}
