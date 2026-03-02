using GamerLFG.Models.ViewModels;

namespace GamerLFG.Models.TestData
{
    /// <summary>
    /// ข้อมูลทดสอบสำหรับ LobbyDetails — ครบทุก 8 กรณี
    ///
    ///  SC1 = HOST              (lobby active, recruiting open)
    ///  SC2 = NOT LOGGED IN     (ยังไม่ได้ login)
    ///  SC3 = VISITOR OPEN      (login แล้ว, recruiting OPEN)
    ///  SC4 = VISITOR CLOSED    (login แล้ว, recruiting CLOSED)
    ///  SC5 = PENDING           (ส่ง request ไปแล้ว รอ)
    ///  SC6 = MEMBER            (ถูกรับเข้าแล้ว, lobby active)
    ///  SC7 = MEMBER+COMPLETED  (Lobby จบแล้ว, โชว์ Karma)
    ///  SC8 = HOST+COMPLETED    (Host + Lobby จบแล้ว)
    /// </summary>
    public static class LobbyTestCases
    {
        // ── Shared fake IDs (24-char hex = valid MongoDB ObjectId format) ───────
        private const string IdHost    = "000000000000000000000001";
        private const string IdMember1 = "000000000000000000000002";
        private const string IdMember2 = "000000000000000000000003";
        private const string IdVisitor = "000000000000000000000099";
        private const string IdLobby   = "aaaaaaaaaaaaaaaaaaaaaaaa";

        // ── Shared fake users ───────────────────────────────────────────────────
        private static User Host => new()
        {
            Id       = IdHost,
            Username = "Notatord_Commander",
            Avatar   = "https://api.dicebear.com/7.x/avataaars/svg?seed=Notatord",
            KarmaScore = 4.8,
        };
        private static User Member1 => new()
        {
            Id       = IdMember1,
            Username = "Dew_The_Slayer",
            Avatar   = "https://api.dicebear.com/7.x/avataaars/svg?seed=Dew",
            KarmaScore = 4.2,
        };
        private static User Member2 => new()
        {
            Id       = IdMember2,
            Username = "TanK_Artisan",
            Avatar   = "https://api.dicebear.com/7.x/avataaars/svg?seed=Tank",
            KarmaScore = 3.9,
        };
        private static User Visitor => new()
        {
            Id       = IdVisitor,
            Username = "Newbie_Player",
            Avatar   = "https://api.dicebear.com/7.x/avataaars/svg?seed=Newbie",
            KarmaScore = 3.0,
        };

        // ── Member maps ─────────────────────────────────────────────────────────
        private static Dictionary<string, User> BaseMap => new()
        {
            { IdHost,    Host    },
            { IdMember1, Member1 },
        };
        private static Dictionary<string, User> ExtendedMap => new()
        {
            { IdHost,    Host    },
            { IdMember1, Member1 },
            { IdMember2, Member2 },
        };

        // ── Lobby factory ───────────────────────────────────────────────────────
        private static Lobby MakeLobby(bool isRecruiting, bool isComplete,
                                       List<LobbyMember> members) => new()
        {
            Id          = IdLobby,
            Title       = "ไต่แรงค์ Valorant คืนนี้ขอคนไม่ตึงมาก",
            Game        = "Valorant",
            Description = "หาเพื่อนร่วมทีมตำแหน่ง Sentinel และ Duelist ครับ เล่นขำๆ เน้นฮา ไม่เน้นหัวร้อน",
            Picture     = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80",
            HostId      = IdHost,
            MaxPlayers  = 5,
            IsRecruiting = isRecruiting,
            IsComplete  = isComplete,
            Moods       = new() { "Chill", "Voice Chat", "Competitive" },
            EndRecruiting = DateTime.Now.AddHours(2),
            StartEvent  = DateTime.Now.AddHours(3),
            EndEvent    = DateTime.Now.AddHours(5),
            DiscordLink = null,
            Members     = members,
        };

        // ── LobbyMember helpers ─────────────────────────────────────────────────
        private static LobbyMember HostSlot    => new() { UserId = IdHost,    Status = "Host",   Role = "Leader"     };
        private static LobbyMember Member1Slot => new() { UserId = IdMember1, Status = "joined", Role = "Controller" };
        private static LobbyMember Member2Slot => new() { UserId = IdMember2, Status = "joined", Role = "Duelist"    };

        // ════════════════════════════════════════════════════════════════════════
        //  SCENARIOS
        // ════════════════════════════════════════════════════════════════════════

        /// <summary>SC1 — HOST (lobby active, recruiting open, มี pending request จาก Visitor)</summary>
        public static LobbyDetailsViewModel Scenario1_Host() => new()
        {
            Lobby             = MakeLobby(isRecruiting: true, isComplete: false,
                                          members: new() { HostSlot, Member1Slot }),
            CurrentUserId     = IdHost,
            IsHost            = true,
            IsMember          = true,
            HasPendingRequest = false,
            MemberMap         = BaseMap,
            PendingApplications = new()
            {
                new LobbyMember { UserId = IdVisitor, Status = "Pending", Role = "Support" }
            },
            ApplicantMap = new() { { IdVisitor, Visitor } },
        };

        /// <summary>SC2 — NOT LOGGED IN (ไม่ได้ login)</summary>
        public static LobbyDetailsViewModel Scenario2_NotLoggedIn() => new()
        {
            Lobby             = MakeLobby(isRecruiting: true, isComplete: false,
                                          members: new() { HostSlot, Member1Slot }),
            CurrentUserId     = null,
            IsHost            = false,
            IsMember          = false,
            HasPendingRequest = false,
            MemberMap         = BaseMap,
        };

        /// <summary>SC3 — VISITOR (login แล้ว, recruiting OPEN)</summary>
        public static LobbyDetailsViewModel Scenario3_VisitorOpen() => new()
        {
            Lobby             = MakeLobby(isRecruiting: true, isComplete: false,
                                          members: new() { HostSlot, Member1Slot }),
            CurrentUserId     = IdVisitor,
            IsHost            = false,
            IsMember          = false,
            HasPendingRequest = false,
            MemberMap         = BaseMap,
        };

        /// <summary>SC4 — VISITOR (login แล้ว, recruiting CLOSED)</summary>
        public static LobbyDetailsViewModel Scenario4_VisitorClosed() => new()
        {
            Lobby             = MakeLobby(isRecruiting: false, isComplete: false,
                                          members: new() { HostSlot, Member1Slot }),
            CurrentUserId     = IdVisitor,
            IsHost            = false,
            IsMember          = false,
            HasPendingRequest = false,
            MemberMap         = BaseMap,
        };

        /// <summary>SC5 — PENDING (ส่ง request ไปแล้ว รอการอนุมัติ)</summary>
        public static LobbyDetailsViewModel Scenario5_Pending() => new()
        {
            Lobby             = MakeLobby(isRecruiting: true, isComplete: false,
                                          members: new() { HostSlot, Member1Slot }),
            CurrentUserId     = IdVisitor,
            IsHost            = false,
            IsMember          = false,
            HasPendingRequest = true,
            MemberMap         = BaseMap,
        };

        /// <summary>SC6 — MEMBER (ถูกรับเข้าแล้ว, lobby ยังเล่นอยู่)</summary>
        public static LobbyDetailsViewModel Scenario6_Member() => new()
        {
            Lobby             = MakeLobby(isRecruiting: true, isComplete: false,
                                          members: new() { HostSlot, Member1Slot }),
            CurrentUserId     = IdMember1,
            IsHost            = false,
            IsMember          = true,
            HasPendingRequest = false,
            MemberMap         = BaseMap,
        };

        /// <summary>SC7 — MEMBER + COMPLETED (Lobby จบแล้ว, โชว์ Karma Evaluation)</summary>
        public static LobbyDetailsViewModel Scenario7_MemberCompleted() => new()
        {
            Lobby             = MakeLobby(isRecruiting: false, isComplete: true,
                                          members: new() { HostSlot, Member1Slot, Member2Slot }),
            CurrentUserId     = IdMember1,
            IsHost            = false,
            IsMember          = true,
            HasPendingRequest = false,
            MemberMap         = ExtendedMap,
            // Member1 เคย rate Member2 ไปแล้ว → แสดง "Evaluated"
            EndorsedUserIds   = new() { IdMember2 },
        };

        /// <summary>SC8 — HOST + COMPLETED (ไม่มีปุ่ม Complete Mission)</summary>
        public static LobbyDetailsViewModel Scenario8_HostCompleted() => new()
        {
            Lobby             = MakeLobby(isRecruiting: false, isComplete: true,
                                          members: new() { HostSlot, Member1Slot, Member2Slot }),
            CurrentUserId     = IdHost,
            IsHost            = true,
            IsMember          = true,
            HasPendingRequest = false,
            MemberMap         = ExtendedMap,
        };

        // ── Convenience: array ให้ loop ทดสอบทั้งหมดได้ง่าย ───────────────────
        public static IEnumerable<(int No, string Label, LobbyDetailsViewModel VM)> All() =>
        new[]
        {
            (1, "HOST",              Scenario1_Host()),
            (2, "NOT LOGGED IN",     Scenario2_NotLoggedIn()),
            (3, "VISITOR (OPEN)",    Scenario3_VisitorOpen()),
            (4, "VISITOR (CLOSED)",  Scenario4_VisitorClosed()),
            (5, "PENDING",           Scenario5_Pending()),
            (6, "MEMBER",            Scenario6_Member()),
            (7, "MEMBER+COMPLETED",  Scenario7_MemberCompleted()),
            (8, "HOST+COMPLETED",    Scenario8_HostCompleted()),
        };
    }
}
