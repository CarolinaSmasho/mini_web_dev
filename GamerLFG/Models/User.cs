using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamerLFG.Models
{
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } // ในใช้งานจริงควรเก็บเป็น Hash

    // Profile (Embedded Fields)
    public string Avatar { get; set; }
    public string Bio { get; set; }
    public double KarmaScore { get; set; }
    public List<string> VibeTags { get; set; } = new();
    public List<string> GameLibrary { get; set; } = new();
    
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> FriendIds { get; set; } = new();
    
    public List<string> SocialMedia { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}