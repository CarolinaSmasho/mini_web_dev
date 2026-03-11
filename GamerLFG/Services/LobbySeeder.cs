using GamerLFG.Models;
using GamerLFG.service;
using MongoDB.Driver;

namespace GamerLFG.Services
{

    public class LobbySeeder
    {

        public const string IdHost    = "000000000000000000000001";
        public const string IdMember1 = "000000000000000000000002";
        public const string IdMember2 = "000000000000000000000003";
        public const string IdVisitor = "000000000000000000000098";
        public const string IdPending = "000000000000000000000099";

        public const string IdLobby1_BeforeRecruit = "aaaaaaaaaaaaaaaaaaaaa001";
        public const string IdLobby2_Recruiting    = "aaaaaaaaaaaaaaaaaaaaa002";
        public const string IdLobby3_Intermission  = "aaaaaaaaaaaaaaaaaaaaa003";
        public const string IdLobby4_Mission       = "aaaaaaaaaaaaaaaaaaaaa004";
        public const string IdLobby5_Completed     = "aaaaaaaaaaaaaaaaaaaaa005";
        public const string IdLobby = "aaaaaaaaaaaaaaaaaaaaaaaa";

        public static async Task SeedAsync(MongoDBservice db)
        {
            Console.WriteLine("[LobbySeeder] กำลังตรวจสอบข้อมูลทดสอบ...");

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
                StartRecruiting = now.AddDays(1),
                EndRecruiting   = now.AddDays(1).AddHours(3),
                StartEvent      = now.AddDays(2),
                EndEvent        = now.AddDays(2).AddHours(3),
                Members         = new() { new() { UserId = IdHost, Status = "Host", Role = "Leader" } },
                CreatedAt       = now.AddDays(-1),
            };

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
                StartRecruiting = now.AddHours(-2),
                EndRecruiting   = now.AddHours(4),
                StartEvent      = now.AddDays(1),
                EndEvent        = now.AddDays(1).AddHours(3),
                Members         = new(defaultMembers),
                CreatedAt       = now.AddHours(-3),
            };

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
                StartRecruiting = now.AddDays(-2),
                EndRecruiting   = now.AddHours(-5),
                StartEvent      = now.AddHours(3),
                EndEvent        = now.AddHours(6),
                Members         = new(defaultMembers),
                CreatedAt       = now.AddDays(-3),
            };

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
                StartEvent      = now.AddHours(-1),
                EndEvent        = now.AddHours(2),
                Members         = new(defaultMembers),
                CreatedAt       = now.AddDays(-4),
            };

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
                EndEvent        = now.AddDays(-5).AddHours(3),
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

            Console.WriteLine("[LobbySeeder] กำลัง upsert Applications...");

            Console.WriteLine("[LobbySeeder] กำลัง upsert KarmaHistory...");

            var karmaHistories = new List<KarmaHistory>
            {

                new() {
                    Id           = "cccccccccccccccccccccc01",
                    TargetUserId = IdMember1,
                    FromUserId   = IdHost,
                    Score        = 5,
                    Comment      = "DPS เทพมาก carry ทั้งทีม",
                    Date         = now.AddDays(-5).AddHours(4),
                },

                new() {
                    Id           = "cccccccccccccccccccccc02",
                    TargetUserId = IdMember2,
                    FromUserId   = IdHost,
                    Score        = 4,
                    Comment      = "Tank ได้ดี แต่บางทีหลุดตำแหน่ง",
                    Date         = now.AddDays(-5).AddHours(4),
                },

                new() {
                    Id           = "cccccccccccccccccccccc03",
                    TargetUserId = IdHost,
                    FromUserId   = IdMember1,
                    Score        = 5,
                    Comment      = "ลีดเก่งมาก สั่งงานชัดเจน",
                    Date         = now.AddDays(-5).AddHours(5),
                },

                new() {
                    Id           = "cccccccccccccccccccccc04",
                    TargetUserId = IdHost,
                    FromUserId   = IdMember2,
                    Score        = 4,
                    Comment      = "ลีดโอเค แต่ตึงไปนิดนึง",
                    Date         = now.AddDays(-5).AddHours(5),
                },

                new() {
                    Id           = "cccccccccccccccccccccc05",
                    TargetUserId = IdMember2,
                    FromUserId   = IdMember1,
                    Score        = 3,
                    Comment      = "เล่นได้พอใช้ ยังต้องฝึกอีก",
                    Date         = now.AddDays(-5).AddHours(5),
                },
            };

            foreach (var karma in karmaHistories)
            {
                await db.KarmaHistories.ReplaceOneAsync(
                    k => k.Id == karma.Id,
                    karma,
                    new ReplaceOptions { IsUpsert = true });
            }
            Console.WriteLine($"[LobbySeeder] upserted {karmaHistories.Count} karma histories");

            var allSeededUserIds = users.Select(u => u.Id).ToList();
            foreach (var uid in allSeededUserIds)
            {
                var histories = await db.KarmaHistories
                    .Find(k => k.TargetUserId == uid)
                    .ToListAsync();

                if (histories.Any())
                {
                    var avg = Math.Round(histories.Average(k => k.Score), 2);
                    var uf  = Builders<User>.Filter.Eq(u => u.Id, uid);
                    var uu  = Builders<User>.Update.Set(u => u.KarmaScore, avg);
                    await db.Users.UpdateOneAsync(uf, uu);
                    Console.WriteLine($"[LobbySeeder] KarmaScore for {uid} → {avg:0.00}");
                }
            }


            Console.WriteLine("[LobbySeeder] กำลัง upsert Notifications...");

            var notifications = new List<Notification>
            {

                new() {
                    Id             = "dddddddddddddddddddddd01",
                    Type           = "friend_request",
                    RelateObjectId = "eeeeeeeeeeeeeeeeeeeeee01",
                    UserId         = IdHost,
                    Text           = "Newbie_Player ส่งคำขอเป็นเพื่อน",
                    IsRead         = false,
                    Date           = now.AddHours(-2),
                },

                new() {
                    Id             = "dddddddddddddddddddddd02",
                    Type           = "lobby_invite",
                    RelateObjectId = IdLobby2_Recruiting,
                    UserId         = IdVisitor,
                    Text           = "Notatord_Commander เชิญคุณเข้า Lobby: หาทีม Elden Ring Co-op",
                    IsRead         = false,
                    Date           = now.AddHours(-1),
                },

                new() {
                    Id             = "dddddddddddddddddddddd03",
                    Type           = "lobby_invite",
                    RelateObjectId = IdLobby2_Recruiting,
                    UserId         = IdPending,
                    Text           = "Notatord_Commander เชิญคุณเข้า Lobby: หาทีม Elden Ring Co-op",
                    IsRead         = true,
                    Date           = now.AddHours(-3),
                },

                new() {
                    Id             = "dddddddddddddddddddddd04",
                    Type           = "friend_request",
                    RelateObjectId = "eeeeeeeeeeeeeeeeeeeeee02",
                    UserId         = IdMember1,
                    Text           = "Notatord_Commander ตอบรับคำขอเป็นเพื่อนแล้ว",
                    IsRead         = true,
                    Date           = now.AddDays(-3),
                },

                new() {
                    Id             = "dddddddddddddddddddddd05",
                    Type           = "application_accepted",
                    RelateObjectId = IdLobby3_Intermission,
                    UserId         = IdMember1,
                    Text           = "คุณได้รับการอนุมัติเข้า Lobby: รอเริ่ม League of Legends",
                    IsRead         = true,
                    Date           = now.AddDays(-2).AddHours(3),
                },

                new() {
                    Id             = "dddddddddddddddddddddd06",
                    Type           = "application_rejected",
                    RelateObjectId = IdLobby3_Intermission,
                    UserId         = IdVisitor,
                    Text           = "คำขอเข้า Lobby: รอเริ่ม League of Legends ถูกปฏิเสธ",
                    IsRead         = false,
                    Date           = now.AddDays(-2).AddHours(4),
                },

                new() {
                    Id             = "dddddddddddddddddddddd07",
                    Type           = "new_application",
                    RelateObjectId = IdLobby2_Recruiting,
                    UserId         = IdHost,
                    Text           = "Newbie_Player สมัครเข้า Lobby: หาทีม Elden Ring Co-op",
                    IsRead         = false,
                    Date           = now.AddHours(-1),
                },
            };

            foreach (var noti in notifications)
            {
                await db.Notifications.ReplaceOneAsync(
                    n => n.Id == noti.Id,
                    noti,
                    new ReplaceOptions { IsUpsert = true });
            }
            Console.WriteLine($"[LobbySeeder] upserted {notifications.Count} notifications");

            Console.WriteLine("[LobbySeeder] กำลัง upsert FriendRequests...");

            var friendRequestCollection = db.Users.Database
                .GetCollection<FriendRequest>("FriendRequests");

            var friendRequests = new List<FriendRequest>
            {

                new() {
                    Id           = "eeeeeeeeeeeeeeeeeeeeee01",
                    UserSender   = IdPending,
                    UserReceiver = IdHost,
                    Status       = "pending",
                    CreatedAt    = now.AddHours(-2),
                },

                new() {
                    Id           = "eeeeeeeeeeeeeeeeeeeeee02",
                    UserSender   = IdMember1,
                    UserReceiver = IdHost,
                    Status       = "accepted",
                    CreatedAt    = now.AddMonths(-4),
                },

                new() {
                    Id           = "eeeeeeeeeeeeeeeeeeeeee03",
                    UserSender   = IdHost,
                    UserReceiver = IdMember2,
                    Status       = "accepted",
                    CreatedAt    = now.AddMonths(-3),
                },

                new() {
                    Id           = "eeeeeeeeeeeeeeeeeeeeee04",
                    UserSender   = IdMember2,
                    UserReceiver = IdMember1,
                    Status       = "accepted",
                    CreatedAt    = now.AddMonths(-2),
                },

                new() {
                    Id           = "eeeeeeeeeeeeeeeeeeeeee05",
                    UserSender   = IdVisitor,
                    UserReceiver = IdMember1,
                    Status       = "pending",
                    CreatedAt    = now.AddDays(-1),
                },
            };

            foreach (var fr in friendRequests)
            {
                await friendRequestCollection.ReplaceOneAsync(
                    f => f.Id == fr.Id,
                    fr,
                    new ReplaceOptions { IsUpsert = true });
            }
            Console.WriteLine($"[LobbySeeder] upserted {friendRequests.Count} friend requests");

            Console.WriteLine("[LobbySeeder] ✅ Seed ทั้งหมดเสร็จสมบูรณ์!");
        }
    }
}
