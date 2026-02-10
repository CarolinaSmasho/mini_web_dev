using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GamerLFG.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Plain text (should be hashed in prod)
        public string Bio { get; set; } = "No bio yet.";
        public string AvatarUrl { get; set; } = "https://ui-avatars.com/api/?background=random";
        
        public double KarmaScore { get; set; } = 0.0;
        
        public List<string> VibeTags { get; set; } = new List<string>();
        public List<string> GameLibrary { get; set; } = new List<string>();
        
        // Keeping FriendIds for potential future use or migration, though distinct friend collection is better
        public List<string> FriendIds { get; set; } = new List<string>();
        
        public string DiscordUserId { get; set; } = string.Empty;
        public string SteamId { get; set; } = string.Empty;
        public string TwitchChannel { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsOnline { get; set; } = false;
    }
}
