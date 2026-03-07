using GamerLFG.Models;
using GamerLFG.service;
using MongoDB.Driver;

namespace GamerLFG.Services
{
    /// <summary>
    /// Seed ข้อมูลทดสอบสำหรับ Lobby/Details
    /// รันครั้งเดียวตอน startup — ถ้ามีข้อมูลอยู่แล้วจะข้ามไป
    /// </summary>
    public class LobbySeeder
    {
        // ── IDs ตรงกับ LobbyTestCases.cs ──────────────────────────────────────
        public const string IdHost    = "000000000000000000000001";
        public const string IdMember1 = "000000000000000000000002";
        public const string IdMember2 = "000000000000000000000003";
        public const string IdVisitor = "000000000000000000000098"; // SC3/SC4 — ยังไม่ได้ขอเข้า
        public const string IdPending = "000000000000000000000099"; // SC5     — ส่ง request ไปแล้ว

        // ── Lobby IDs (5 สถานการณ์) ─────────────────────────────────────────
        public const string IdLobby1_BeforeRecruit = "aaaaaaaaaaaaaaaaaaaaa001";
        public const string IdLobby2_Recruiting    = "aaaaaaaaaaaaaaaaaaaaa002";
        public const string IdLobby3_Intermission  = "aaaaaaaaaaaaaaaaaaaaa003";
        public const string IdLobby4_Mission       = "aaaaaaaaaaaaaaaaaaaaa004";
        public const string IdLobby5_Completed     = "aaaaaaaaaaaaaaaaaaaaa005";
        public const string IdLobby = "aaaaaaaaaaaaaaaaaaaaaaaa"; // legacy

        public static async Task SeedAsync(MongoDBservice db)
        {
            Console.WriteLine("[LobbySeeder] กำลังตรวจสอบข้อมูลทดสอบ...");

            // ── Users (upsert เสมอ — ไม่ขึ้นกับ lobby) ───────────────────────
            var users = new List<User>
            {
                new() {
                    Id = IdHost, Username = "Notatord_Commander",
                    Name = "Notatord",
                    Email = "host@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Notatord_Commander",
                    Bio = "Guild leader since 2015. I lead, you follow.",
                    KarmaScore = 4.8,
                    VibeTags = new() { "Competitive", "Tryhard", "Leader" },
                    GameLibrary = new() { "Valorant", "Elden Ring", "League of Legends", "Overwatch 2", "Minecraft" },
                    FriendIds = new() { IdMember1, IdMember2 },
                    discord = "Notatord#1234",
                    steam = "notatord_cmd",
                    twitch = "notatord_live",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                },
                new() {
                    Id = IdMember1, Username = "Dew_The_Slayer",
                    Name = "Dew",
                    Email = "member1@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Dew_The_Slayer",
                    Bio = "DPS main. If it moves, it dies.",
                    KarmaScore = 4.2,
                    VibeTags = new() { "Competitive", "Chill" },
                    GameLibrary = new() { "Valorant", "Elden Ring", "CS2" },
                    FriendIds = new() { IdHost, IdMember2 },
                    discord = "DewSlayer#5678",
                    steam = "dew_slayer",
                    twitch = "dew_plays",
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                },
                new() {
                    Id = IdMember2, Username = "TanK_Artisan",
                    Name = "Tank",
                    Email = "member2@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=TanK_Artisan",
                    Bio = "I tank so you don't have to. Shield up!",
                    KarmaScore = 3.9,
                    VibeTags = new() { "Chill", "Casual" },
                    GameLibrary = new() { "Overwatch 2", "League of Legends", "Minecraft" },
                    FriendIds = new() { IdHost, IdMember1 },
                    discord = "TankArt#9012",
                    steam = "tank_artisan",
                    twitch = "",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                },
                new() {
                    Id = IdVisitor, Username = "Looker_Guy",
                    Name = "Looker",
                    Email = "visitor@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Looker_Guy",
                    Bio = "Just browsing lobbies. Might join if the vibe is right.",
                    KarmaScore = 3.5,
                    VibeTags = new() { "Casual" },
                    GameLibrary = new() { "Minecraft", "Stardew Valley" },
                    FriendIds = new(),
                    discord = "Looker#3456",
                    steam = "looker_guy",
                    twitch = "",
                    CreatedAt = DateTime.UtcNow.AddMonths(-1),
                },
                new() {
                    Id = IdPending, Username = "Newbie_Player",
                    Name = "Newbie",
                    Email = "pending@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Newbie_Player",
                    Bio = "New to gaming, looking for friends!",
                    KarmaScore = 3.0,
                    VibeTags = new() { "Casual", "Friendly" },
                    GameLibrary = new() { "Valorant" },
                    FriendIds = new(),
                    discord = "Newbie#7890",
                    steam = "newbie_player",
                    twitch = "",
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                },
            };

            foreach (var user in users)
            {
                await db.Users.ReplaceOneAsync(
                    u => u.Id == user.Id,
                    user,
                    new ReplaceOptions { IsUpsert = true });
                Console.WriteLine($"[LobbySeeder] upserted user: {user.Username}");
            }

            // ── Lobbies — upsert ทุกครั้งเพื่อให้เวลาอ้างอิงจาก now ปัจจุบัน ──
            var now = DateTime.UtcNow;

            Console.WriteLine("[LobbySeeder] กำลัง upsert lobbies 5 สถานการณ์...");

            var defaultMembers = new List<LobbyMember>
            {
                new() { UserId = IdHost,    Status = "Host",    Role = "Leader"     },
                new() { UserId = IdMember1, Status = "joined",  Role = "Controller" },
                new() { UserId = IdPending, Status = "Pending", Role = "Controller" },
            };

            var defaultRoles = new List<LobbyRole>
            {
                new() { Name = "Leader",     Quantity = 1 },
                new() { Name = "Controller", Quantity = 2 },
            };

            // ─── 1) ก่อน Recruit — ยังไม่ถึงเวลารับสมัคร ──────────────────
            var lobby1 = new Lobby
            {
                Id              = IdLobby1_BeforeRecruit,
                Title           = "[Pre-Recruit] รอเปิดรับสมัคร Valorant",
                Game            = "Valorant",
                Description     = "ห้องนี้ยังไม่เปิดรับสมัคร รอสักครู่นะครับ",
                Picture         = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80",
                HostId          = IdHost,
                HostName        = "Notatord_Commander",
                MaxPlayers      = 5,
                IsRecruiting    = false,
                IsComplete      = false,
                Moods           = new() { "Chill" },
                Roles           = new(defaultRoles),
                DiscordLink     = null,
                StartRecruiting = now.AddDays(1),              // พรุ่งนี้
                EndRecruiting   = now.AddDays(1).AddHours(3),
                StartEvent      = now.AddDays(2),
                EndEvent        = now.AddDays(2).AddHours(3),
                Members         = new() { new() { UserId = IdHost, Status = "Host", Role = "Leader" } },
                CreatedAt       = now.AddDays(-1),
            };

            // ─── 2) ช่วง Recruit — กำลังเปิดรับสมัคร ──────────────────────
            var lobby2 = new Lobby
            {
                Id              = IdLobby2_Recruiting,
                Title           = "[Recruiting] หาทีม Elden Ring Co-op",
                Game            = "Elden Ring",
                Description     = "เปิดรับสมัครอยู่ครับ เข้ามาเลย!",
                Picture         = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80",
                HostId          = IdHost,
                HostName        = "Notatord_Commander",
                MaxPlayers      = 5,
                IsRecruiting    = true,
                IsComplete      = false,
                Moods           = new() { "Chill", "Competitive" },
                Roles           = new(defaultRoles),
                DiscordLink     = null,
                StartRecruiting = now.AddHours(-2),            // เริ่มไปแล้ว 2 ชม.
                EndRecruiting   = now.AddHours(4),             // ปิดรับอีก 4 ชม.
                StartEvent      = now.AddDays(1),
                EndEvent        = now.AddDays(1).AddHours(3),
                Members         = new(defaultMembers),
                CreatedAt       = now.AddHours(-3),
            };

            // ─── 3) หลัง Recruit / Intermission — ปิดรับแล้ว รอเริ่มงาน ───
            var lobby3 = new Lobby
            {
                Id              = IdLobby3_Intermission,
                Title           = "[Intermission] รอเริ่ม League of Legends",
                Game            = "League of Legends",
                Description     = "ปิดรับสมัครแล้ว รอเวลาเริ่มแข่งครับ",
                Picture         = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80",
                HostId          = IdHost,
                HostName        = "Notatord_Commander",
                MaxPlayers      = 5,
                IsRecruiting    = false,
                IsComplete      = false,
                Moods           = new() { "Competitive" },
                Roles           = new(defaultRoles),
                DiscordLink     = "https://discord.gg/example",
                StartRecruiting = now.AddDays(-2),             // เปิดรับเมื่อ 2 วันก่อน
                EndRecruiting   = now.AddHours(-5),            // ปิดรับไปแล้ว 5 ชม.
                StartEvent      = now.AddHours(3),             // เริ่มอีก 3 ชม.
                EndEvent        = now.AddHours(6),
                Members         = new(defaultMembers),
                CreatedAt       = now.AddDays(-3),
            };

            // ─── 4) ช่วง Mission — กำลังเล่นอยู่ ──────────────────────────
            var lobby4 = new Lobby
            {
                Id              = IdLobby4_Mission,
                Title           = "[In Mission] กำลังบุก Overwatch 2",
                Game            = "Overwatch 2",
                Description     = "กำลังเล่นอยู่ครับ ห้ามรบกวน!",
                Picture         = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80",
                HostId          = IdHost,
                HostName        = "Notatord_Commander",
                MaxPlayers      = 5,
                IsRecruiting    = false,
                IsComplete      = false,
                Moods           = new() { "Competitive", "Tryhard" },
                Roles           = new(defaultRoles),
                DiscordLink     = "https://discord.gg/example2",
                StartRecruiting = now.AddDays(-3),
                EndRecruiting   = now.AddDays(-2),
                StartEvent      = now.AddHours(-1),            // เริ่มไปแล้ว 1 ชม.
                EndEvent        = now.AddHours(2),             // จบอีก 2 ชม.
                Members         = new(defaultMembers),
                CreatedAt       = now.AddDays(-4),
            };

            // ─── 5) หลัง Mission — จบแล้ว ──────────────────────────────────
            var lobby5 = new Lobby
            {
                Id              = IdLobby5_Completed,
                Title           = "[Completed] จบแล้ว Minecraft Raid",
                Game            = "Minecraft",
                Description     = "ภารกิจเสร็จสิ้นแล้วครับ ขอบคุณทุกคน!",
                Picture         = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80",
                HostId          = IdHost,
                HostName        = "Notatord_Commander",
                MaxPlayers      = 5,
                IsRecruiting    = false,
                IsComplete      = true,
                Moods           = new() { "Chill" },
                Roles           = new(defaultRoles),
                DiscordLink     = "https://discord.gg/example3",
                StartRecruiting = now.AddDays(-7),
                EndRecruiting   = now.AddDays(-6),
                StartEvent      = now.AddDays(-5),
                EndEvent        = now.AddDays(-5).AddHours(3), // จบไปแล้ว 5 วัน
                Members         = new(defaultMembers),
                CreatedAt       = now.AddDays(-8),
            };

            var lobbies = new[] { lobby1, lobby2, lobby3, lobby4, lobby5 };
            foreach (var lobby in lobbies)
            {
                await db.Lobbies.ReplaceOneAsync(
                    l => l.Id == lobby.Id,
                    lobby,
                    new ReplaceOptions { IsUpsert = true });
            }

            Console.WriteLine("[LobbySeeder] Seed สำเร็จ! upsert 5 lobbies ทุกสถานการณ์");
        }
    }
}
