using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GamerLFG.Models
{
    public class Lobby
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        
        public string Title { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // HostId is crucial for querying lobbies hosted by a specific user
        public string HostId { get; set; } = string.Empty;
        
        public int MaxPlayers { get; set; } = 5;
        
        public List<string> Moods { get; set; } = new List<string>();
        
        public List<Role> Roles { get; set; } = new List<Role>();
        
        public List<Member> Members { get; set; } = new List<Member>();
        
        // Helper to get count of current players
        [BsonIgnore]
        public int CurrentPlayers => Members?.Count ?? 0;
        
        // Legacy support if needed, otherwise rely on Members
        [BsonIgnore]
        public List<string> PlayerIds => Members?.Select(m => m.UserId).ToList() ?? new List<string>();

        public string Status { get; set; } = "Open"; // Open, Full, InGame, Closed
        public bool IsRecruiting { get; set; } = true;
        
        public string PictureUrl { get; set; } = string.Empty;
        public string DiscordLink { get; set; } = string.Empty;

        public DateTime? ScheduledTime { get; set; }
        public DateTime? RecruitmentDeadline { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
