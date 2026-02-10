using System;

namespace GamerLFG.Models
{
    public class Member
    {
        public string UserId { get; set; } = string.Empty;
        public string AssignedRole { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
