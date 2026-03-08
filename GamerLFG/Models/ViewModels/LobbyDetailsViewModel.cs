using GamerLFG.Models;

namespace GamerLFG.Models.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับหน้า Lobby/Details รวมทุกข้อมูลที่ View ต้องการ
    /// </summary>
    public class LobbyDetailsViewModel
    {
        // ── Lobby ────────────────────────────────────────────
        public Lobby Lobby { get; set; } = new();

        // ── Auth context ─────────────────────────────────────
        /// <summary>UserId ของคนที่ login อยู่ (null = ยังไม่ได้ login)</summary>
        public string? CurrentUserId { get; set; }
        /// <summary>User object ของคนที่ login อยู่ (null = ยังไม่ได้ login)</summary>
        public User? CurrentUser { get; set; }

        /// <summary>Username ของคนที่ login อยู่ สำหรับแสดงผล</summary>
        public string CurrentUsername =>
            CurrentUser?.Username ?? CurrentUserId ?? "Not logged in";

        // ── Role flags (derived จาก Controller) ──────────────
        public bool IsHost { get; set; }
        /// <summary>true = เป็น member ที่ถูกรับเข้าแล้ว (รวม host)</summary>
        public bool IsMember { get; set; }
        public bool HasPendingRequest { get; set; }

        // ── Member data ───────────────────────────────────────
        /// <summary>UserId → User (สำหรับแสดงชื่อ/avatar ของแต่ละ slot)</summary>
        public Dictionary<string, User> MemberMap { get; set; } = new();

        // ── Pending members (มองเห็นเฉพาะ Host) ──────────────
        /// <summary>Members ที่ Status == "Pending" (ยังไม่ถูกรับ)</summary>
        public List<LobbyMember> PendingApplications { get; set; } = new();
        /// <summary>UserId → User ของผู้สมัครที่รอการอนุมัติ</summary>
        public Dictionary<string, User> ApplicantMap { get; set; } = new();

        // ── Karma ─────────────────────────────────────────────
        /// <summary>UserId ที่ currentUser ได้ rate ไปแล้ว (เพื่อ render "Evaluated")</summary>
        public HashSet<string> EndorsedUserIds { get; set; } = new();

        // ── Friend Invite ───────────────────────────────────────
        /// <summary>เพื่อนของ currentUser ที่ยังไม่อยู่ใน lobby (สำหรับ invite UI)</summary>
        public List<User> InvitableFriends { get; set; } = new();
        /// <summary>true = currentUser ถูกเชิญแต่ยังไม่ตอบรับ (Status == "Invited")</summary>
        public bool HasPendingInvite { get; set; }
        /// <summary>ชื่อคนที่เชิญ currentUser (สำหรับแสดงในปุ่มตอบรับ)</summary>
        public string? InvitedByName { get; set; }

        // ── Computed helpers ──────────────────────────────────
        /// <summary>รายการ UserId ของสมาชิกที่เข้าร่วมจริง (ไม่รวม Pending)</summary>
        public List<string> PlayerIds =>
            Lobby.Members
                 .Where(m => m.Status != "Pending")
                 .Select(m => m.UserId)
                 .ToList();

        /// <summary>ชื่อ Host (ดึงจาก MemberMap — ไม่มี fallback ไป HostName ใน DB แล้ว)</summary>
        public string HostName =>
            MemberMap.TryGetValue(Lobby.HostId ?? "", out var h)
                ? h.Username
                : (Lobby.HostId ?? "Unknown");
    }
}
