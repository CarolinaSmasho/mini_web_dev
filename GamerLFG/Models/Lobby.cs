using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;

namespace GamerLFG.Models
{
public class Lobby
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Title { get; set; }
    public string Game { get; set; }
    public string Description { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string HostId { get; set; }
    public string HostName {get; set;}
    public string Picture { get; set; }
    public string DiscordLink { get; set; }

    // Settings & Tags
    public List<string> Moods { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public int MaxPlayers { get; set; }
    public bool IsRecruiting { get; set; }
    public bool IsComplete { get; set; }

    // Time Management
    public DateTime StartRecruiting { get; set; }
    public DateTime EndRecruiting { get; set; }
    public DateTime StartEvent { get; set; }
    public DateTime EndEvent { get; set; }

    // Members (Embedded Array)
    public List<LobbyMember> Members { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LobbyMember
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string Status { get; set; } // e.g., 'joined', 'pending'
    public string Role { get; set; }
}

}
