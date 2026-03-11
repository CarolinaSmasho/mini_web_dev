using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GamerLFG.Models
{
public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    
    public string? Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }

    public string? PasswordHash { get; set; }

    public string Avatar { get; set; }
    public string Bio { get; set; }
    public double KarmaScore { get; set; }
    public List<string> VibeTags { get; set; } = new();
    public List<string> GameLibrary { get; set; } = new();
    
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> FriendIds { get; set; } = new();
    
    public string discord { get; set; }
    public string steam { get; set; }
    public string twitch { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}