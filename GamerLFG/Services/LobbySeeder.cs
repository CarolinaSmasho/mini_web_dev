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
        public const string IdLobby   = "aaaaaaaaaaaaaaaaaaaaaaaa";

        public static async Task SeedAsync(MongoDBservice db)
        {
            Console.WriteLine("[LobbySeeder] กำลังตรวจสอบข้อมูลทดสอบ...");

            // ── Users (upsert เสมอ — ไม่ขึ้นกับ lobby) ───────────────────────
            var users = new List<User>
            {
                new() {
                    Id = IdHost, Username = "Notatord_Commander",
                    Email = "host@test.com", Password = "test",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Notatord_Commander",
                    KarmaScore = 4.8,
                },
                new() {
                    Id = IdMember1, Username = "Dew_The_Slayer",
                    Email = "member1@test.com", Password = "test",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Dew_The_Slayer",
                    KarmaScore = 4.2,
                },
                new() {
                    Id = IdMember2, Username = "TanK_Artisan",
                    Email = "member2@test.com", Password = "test",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=TanK_Artisan",
                    KarmaScore = 3.9,
                },
                new() {
                    Id = IdVisitor, Username = "Looker_Guy",
                    Email = "visitor@test.com", Password = "test",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Looker_Guy",
                    KarmaScore = 3.5,
                },
                new() {
                    Id = IdPending, Username = "Newbie_Player",
                    Email = "pending@test.com", Password = "test",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Newbie_Player",
                    KarmaScore = 3.0,
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

            // ── Lobby — insert เฉพาะตอนที่ยังไม่มี ───────────────────────────
            var lobbyExists = await db.Lobbies.Find(l => l.Id == IdLobby).AnyAsync();
            if (lobbyExists)
            {
                Console.WriteLine("[LobbySeeder] lobby มีอยู่แล้ว ข้าม");
                return;
            }

            Console.WriteLine("[LobbySeeder] กำลัง insert lobby + application...");
            // ── Lobby (active, recruiting open) ──────────────────────────────
            var lobby = new Lobby
            {
                Id          = IdLobby,
                Title       = "ไต่แรงค์ Valorant คืนนี้ขอคนไม่ตึงมาก",
                Game        = "Valorant",
                Description = "หาเพื่อนร่วมทีมตำแหน่ง Sentinel และ Duelist ครับ เล่นขำๆ เน้นฮา ไม่เน้นหัวร้อน",
                Picture     = "https://images.unsplash.com/photo-1542751371-adc38448a05e?auto=format&fit=crop&w=1200&q=80",
                HostId      = IdHost,
                MaxPlayers  = 5,
                IsRecruiting = true,
                IsComplete  = false,
                Moods       = new() { "Chill", "Voice Chat", "Competitive" },
                DiscordLink = null,
                EndRecruiting = DateTime.Now.AddHours(2),
                StartEvent  = DateTime.Now.AddHours(3),
                EndEvent    = DateTime.Now.AddHours(5),
                Members = new()
                {
                    new() { UserId = IdHost,    Status = "Host",    Role = "Leader"     },
                    new() { UserId = IdMember1, Status = "joined",  Role = "Controller" },
                    new() { UserId = IdPending, Status = "Pending", Role = "Support"    }, // SC5
                },
            };

            await db.Lobbies.InsertOneAsync(lobby);

            Console.WriteLine($"[LobbySeeder] Seed สำเร็จ! Lobby ID = {IdLobby}");
        }
    }
}
