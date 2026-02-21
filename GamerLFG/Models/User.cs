using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamerLFG.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? AvatarUrl { get; set; }
        
        // ข้อมูลโซเชียลที่ใช้ในหน้า View
        public string? DiscordUserId { get; set; }
        public string? SteamId { get; set; }
        public string? TwitchChannel { get; set; }
        
        public double Karma { get; set; } = 0.0;
    }
}