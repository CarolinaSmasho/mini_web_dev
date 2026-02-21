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
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Game { get; set; }
        public string? Description { get; set; }
        public string? PictureUrl { get; set; }
        public string? HostId { get; set; }

        public bool IsRecruiting { get; set; }
        public bool IsCompleted { get; set; }

        // ส่วนที่เก็บข้อมูลสมาชิกและบทบาท
        public List<Role> Roles { get; set; } = new List<Role>();
        public List<Member> Members { get; set; } = new List<Member>();
        public List<string> PlayerIds { get; set; } = new List<string>();
        
        // สำหรับหน้าจอ Mission Protocols
        public List<string> Moods { get; set; } = new List<string>();

        // ข้อมูลเวลา
        public DateTime? RecruitmentDeadline { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }

        // ลิงก์สื่อสาร
        public string? DiscordLink { get; set; }

        // Property พิเศษสำหรับคำนวณจำนวนผู้เล่นปัจจุบัน
        [BsonIgnore]
        public int CurrentPlayers => PlayerIds?.Count ?? 0;
        
        public int MaxPlayers { get; set; }
    }

    // Class เสริมสำหรับจัดการบทบาทในทีม
    public class Role
    {
        public string Name { get; set; } = "";
        public int Count { get; set; }
        public int Filled { get; set; }
    }

    // Class เสริมสำหรับข้อมูลสมาชิกใน Lobby
    public class Member
    {
        public string UserId { get; set; } = "";
        public string AssignedRole { get; set; } = "";
        public bool IsHost { get; set; }
    }
}
