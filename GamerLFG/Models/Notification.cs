using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GamerLFG.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string UserId { get; set; } = string.Empty; // Recipient
        public string Type { get; set; } = "General"; // FriendRequest, Application, Recruitment, System
        public string Message { get; set; } = string.Empty;
        public string? RelatedEntityId { get; set; } // e.g., LobbyId, FriendRequestId
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
