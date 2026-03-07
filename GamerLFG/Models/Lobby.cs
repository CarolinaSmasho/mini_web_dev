using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace GamerLFG.Models
{
    public enum LobbyStatus
    {
        ComingSoon,   // ยังไม่ถึงวัน StartRecruiting
        Recruiting,   // StartRecruiting <= now <= EndRecruiting
        EventOngoing, // EndRecruiting < now <= EndEvent (event กำลังเกิดขึ้น)
        Completed,    // IsComplete = true หรือเลย EndEvent
        Cancelled     // IsRecruiting = false ก่อนถึง StartEvent
    }

public class Lobby
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; }
    public string Game { get; set; }
    public string Description { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string HostId { get; set; }
    public string HostName {get;set;}
    public string Picture { get; set; }
    public string DiscordLink { get; set; }

    // Settings & Tags
    public List<string> Moods { get; set; } = new();

    /// <summary>Roles with quota — each role has a name and max headcount.</summary>
    public List<LobbyRole> Roles { get; set; } = new();

    public int MaxPlayers { get; set; }
    public bool IsRecruiting { get; set; } = true;
    public bool IsComplete { get; set; } = false;

    /// <summary>คำนวณสถานะปัจจุบันของ lobby จากวันที่และ flags</summary>
    public LobbyStatus GetStatus()
    {
        var now = DateTime.UtcNow;

        if (IsComplete || now > EndEvent)
            return LobbyStatus.Completed;

        if (now >= StartEvent)
            return LobbyStatus.EventOngoing;

        if (!IsRecruiting && now < StartEvent)
            return LobbyStatus.Cancelled;

        if (now >= StartRecruiting && now <= EndRecruiting)
            return LobbyStatus.Recruiting;

        return LobbyStatus.ComingSoon;
    }

    // Time Management
    public DateTime StartRecruiting { get; set; }
    public DateTime EndRecruiting { get; set; }
    public DateTime StartEvent { get; set; }
    public DateTime EndEvent { get; set; }

    // Members (Embedded Array)
    public List<LobbyMember> Members { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>A role slot with a name and maximum number of players allowed.</summary>
public class LobbyRole
{
    public string Name { get; set; }
    public int Quantity { get; set; } = 1;
}

public class LobbyMember
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string Status { get; set; } // e.g., 'joined', 'pending'
    public string Role { get; set; }
}

}
