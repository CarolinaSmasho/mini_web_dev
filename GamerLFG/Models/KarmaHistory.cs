using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GamerLFG.Models
{
    public class KarmaHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        
        public string UserId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public string? ReferenceType { get; set; }  // "Lobby", "Endorsement", "Application"
        public string? ReferenceId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Constants for Karma Action Types
    public static class KarmaActionTypes
    {
        public const string ENDORSEMENT_POSITIVE = "ENDORSEMENT_POSITIVE";
        public const string ENDORSEMENT_NEGATIVE = "ENDORSEMENT_NEGATIVE";
        public const string LOBBY_ACCEPTED = "LOBBY_ACCEPTED";
        public const string LOBBY_COMPLETED_HOST = "LOBBY_COMPLETED_HOST";
        public const string LOBBY_COMPLETED_MEMBER = "LOBBY_COMPLETED_MEMBER";
        public const string LOBBY_HARD_KICKED = "LOBBY_HARD_KICKED";
        public const string LOBBY_SOFT_KICKED = "LOBBY_SOFT_KICKED";
        public const string LOBBY_LEFT_EARLY = "LOBBY_LEFT_EARLY";
    }

    // Constants for Karma Points
    public static class KarmaPoints
    {
        public const int ENDORSEMENT_POSITIVE = 10;
        public const int ENDORSEMENT_NEGATIVE = -5;
        public const int LOBBY_ACCEPTED = 5;
        public const int LOBBY_COMPLETED_HOST = 8;
        public const int LOBBY_COMPLETED_MEMBER = 3;
        public const int LOBBY_HARD_KICKED = -10;
        public const int LOBBY_SOFT_KICKED = 0;
        public const int LOBBY_LEFT_EARLY = -3;
    }
}
