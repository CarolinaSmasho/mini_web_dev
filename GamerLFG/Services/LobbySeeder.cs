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
        public const string IdLobby6_OverflowPending = "aaaaaaaaaaaaaaaaaaaaa006";

        public const string IdPending2 = "000000000000000000000010";
        public const string IdPending3 = "000000000000000000000011";
        public const string IdPending4 = "000000000000000000000012";
        public const string IdPending5 = "000000000000000000000013";
        public const string IdPending6 = "000000000000000000000014";
        public const string IdPending7 = "000000000000000000000015";
        public const string IdPending8 = "000000000000000000000016";

        public const string IdHostA = "000000000000000000000020";
        public const string IdHostB = "000000000000000000000021";
        public const string IdHostC = "000000000000000000000022";

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
                new() {
                    Id = IdPending2, Username = "ProSniper_X",
                    Name = "ProSniper",
                    Email = "pending2@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=ProSniper_X",
                    Bio = "Headshot machine. Top 500 every season.",
                    KarmaScore = 4.9,
                    VibeTags = new() { "Competitive", "Tryhard" },
                    GameLibrary = new() { "Valorant", "CS2", "Overwatch 2" },
                    FriendIds = new(),
                    discord = "ProSniper#1111",
                    steam = "prosniper_x",
                    twitch = "prosniper_live",
                    CreatedAt = DateTime.UtcNow.AddMonths(-8),
                },
                new() {
                    Id = IdPending3, Username = "ChillGamer_99",
                    Name = "ChillGamer",
                    Email = "pending3@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=ChillGamer_99",
                    Bio = "Just here to vibe and have fun.",
                    KarmaScore = 3.8,
                    VibeTags = new() { "Chill", "Casual" },
                    GameLibrary = new() { "Valorant", "Minecraft" },
                    FriendIds = new(),
                    discord = "ChillGamer#2222",
                    steam = "chillgamer99",
                    twitch = "",
                    CreatedAt = DateTime.UtcNow.AddMonths(-2),
                },
                new() {
                    Id = IdPending4, Username = "ToxicNoMore",
                    Name = "Reformed",
                    Email = "pending4@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=ToxicNoMore",
                    Bio = "Reformed toxic player. Trying to be better.",
                    KarmaScore = 1.5,
                    VibeTags = new() { "Competitive" },
                    GameLibrary = new() { "Valorant", "League of Legends" },
                    FriendIds = new(),
                    discord = "Reformed#3333",
                    steam = "toxicnomore",
                    twitch = "",
                    CreatedAt = DateTime.UtcNow.AddMonths(-5),
                },
                new() {
                    Id = IdPending5, Username = "SupportMain_Ana",
                    Name = "Ana",
                    Email = "pending5@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=SupportMain_Ana",
                    Bio = "Heal bot reporting for duty. I keep the team alive.",
                    KarmaScore = 4.5,
                    VibeTags = new() { "Chill", "Competitive" },
                    GameLibrary = new() { "Valorant", "Overwatch 2" },
                    FriendIds = new(),
                    discord = "AnaSupport#4444",
                    steam = "supportmain_ana",
                    twitch = "ana_heals",
                    CreatedAt = DateTime.UtcNow.AddMonths(-3),
                },
                new() {
                    Id = IdPending6, Username = "AFKAndy",
                    Name = "Andy",
                    Email = "pending6@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=AFKAndy",
                    Bio = "I might be AFK but my heart is in the game.",
                    KarmaScore = 2.1,
                    VibeTags = new() { "Casual" },
                    GameLibrary = new() { "Valorant" },
                    FriendIds = new(),
                    discord = "AFKAndy#5555",
                    steam = "afkandy",
                    twitch = "",
                    CreatedAt = DateTime.UtcNow.AddDays(-14),
                },
                new() {
                    Id = IdPending7, Username = "FlexKing_MVP",
                    Name = "FlexKing",
                    Email = "pending7@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=FlexKing_MVP",
                    Bio = "I play every role. Need a fill? I'm your guy.",
                    KarmaScore = 4.6,
                    VibeTags = new() { "Competitive", "Chill" },
                    GameLibrary = new() { "Valorant", "League of Legends", "Overwatch 2" },
                    FriendIds = new(),
                    discord = "FlexKing#6666",
                    steam = "flexking_mvp",
                    twitch = "flexking_plays",
                    CreatedAt = DateTime.UtcNow.AddMonths(-4),
                },
                new() {
                    Id = IdPending8, Username = "Lurker_Silent",
                    Name = "Lurker",
                    Email = "pending8@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=Lurker_Silent",
                    Bio = "No mic, but I follow callouts. Trust me.",
                    KarmaScore = 2.8,
                    VibeTags = new() { "Casual" },
                    GameLibrary = new() { "Valorant", "CS2" },
                    FriendIds = new(),
                    discord = "Lurker#7777",
                    steam = "lurker_silent",
                    twitch = "",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                },
                new() {
                    Id = IdHostA, Username = "SakuraRain",
                    Name = "Sakura",
                    Email = "hosta@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=SakuraRain",
                    Bio = "RPG addict. 3000+ hours in every JRPG ever.",
                    KarmaScore = 4.3,
                    VibeTags = new() { "Chill", "Competitive" },
                    GameLibrary = new() { "Final Fantasy XIV", "Genshin Impact", "Valorant", "Monster Hunter" },
                    FriendIds = new(),
                    discord = "Sakura#1001",
                    steam = "sakura_rain",
                    twitch = "sakura_plays",
                    CreatedAt = DateTime.UtcNow.AddMonths(-10),
                },
                new() {
                    Id = IdHostB, Username = "DarkSoulz_69",
                    Name = "DarkSoul",
                    Email = "hostb@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=DarkSoulz_69",
                    Bio = "Git gud or git out. Soulsborne veteran.",
                    KarmaScore = 3.7,
                    VibeTags = new() { "Tryhard", "Competitive" },
                    GameLibrary = new() { "Elden Ring", "Dark Souls 3", "Sekiro", "CS2", "Dota 2" },
                    FriendIds = new(),
                    discord = "DarkSoulz#1002",
                    steam = "darksoulz69",
                    twitch = "darksoulz_live",
                    CreatedAt = DateTime.UtcNow.AddMonths(-7),
                },
                new() {
                    Id = IdHostC, Username = "PixelQueen",
                    Name = "Pixel",
                    Email = "hostc@test.com", PasswordHash = "$2a$11$oVxuDXp/06jDgXNWRib5Q.ojgyRG5VlVLSYcO8A/cpmr0KCogJvS6",
                    Avatar = "https://api.dicebear.com/7.x/avataaars/svg?seed=PixelQueen",
                    Bio = "Indie game enthusiast. Cozy vibes only.",
                    KarmaScore = 4.7,
                    VibeTags = new() { "Chill", "Casual", "Friendly" },
                    GameLibrary = new() { "Stardew Valley", "Minecraft", "Terraria", "Among Us", "Overcooked 2" },
                    FriendIds = new(),
                    discord = "PixelQueen#1003",
                    steam = "pixelqueen",
                    twitch = "pixelqueen_cozy",
                    CreatedAt = DateTime.UtcNow.AddMonths(-5),
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
                Picture         = "https://static.wikia.nocookie.net/valorant/images/8/80/Valorant_Cover_Art.jpg",
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
                Title           = "[Recruiting] หาทีม Elden Cock Ring Co-op",
                Game            = "Elden Ring",
                Description     = "เปิดรับสมัครอยู่ครับ เข้ามาเลย!",
                Picture         = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1245620/header.jpg",
                HostId          = IdHost,
                HostName        = "Notatord_Commander",
                MaxPlayers      = 20,
                IsRecruiting    = true,
                IsComplete      = false,
                Moods           = new() { "Chill", "Competitive" },
                Roles           = new(defaultRoles),
                DiscordLink     = "https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=RDdQw4w9WgXcQ&start_radio=1&pp=ygUJcmljayByb2xsoAcB",
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
                Picture         = "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Lux_0.jpg",
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
                Picture         = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/2357570/header.jpg",
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
                Picture         = "https://minecraft.wiki/images/Minecraft_Key_Art_2024.jpg",
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

            var lobby6 = new Lobby
            {
                Id              = IdLobby6_OverflowPending,
                Title           = "[Overflow] Valorant Ranked 5-Stack",
                Game            = "Valorant",
                Description     = "หาทีม 5 คน แต่คนสมัครเกิน! ระบบจะรับตาม Karma สูงสุด",
                Picture         = "https://static.wikia.nocookie.net/valorant/images/8/80/Valorant_Cover_Art.jpg",
                HostId          = IdHost,
                HostName        = "Notatord_Commander",
                MaxPlayers      = 5,
                IsRecruiting    = true,
                IsComplete      = false,
                AutoRecruitProcessed = false,
                Moods           = new() { "Competitive", "Tryhard" },
                Roles           = new()
                {
                    new() { Name = "Duelist",    Quantity = 2 },
                    new() { Name = "Controller", Quantity = 1 },
                    new() { Name = "Sentinel",   Quantity = 1 },
                    new() { Name = "Initiator",  Quantity = 1 },
                },
                DiscordLink     = "https://discord.gg/example-overflow",
                StartRecruiting = now.AddHours(-6),
                EndRecruiting   = now.AddMinutes(30),
                StartEvent      = now.AddHours(2),
                EndEvent        = now.AddHours(5),
                Members         = new()
                {
                    new() { UserId = IdHost,     Status = "Host",    Role = "Duelist",    AppliedAt = now.AddHours(-6) },
                    new() { UserId = IdMember1,  Status = "joined",  Role = "Controller", AppliedAt = now.AddHours(-5) },
                    new() { UserId = IdPending,  Status = "Pending", Role = "Sentinel",   AppliedAt = now.AddHours(-4) },
                    new() { UserId = IdPending2, Status = "Pending", Role = "Duelist",    AppliedAt = now.AddHours(-4).AddMinutes(5) },
                    new() { UserId = IdPending3, Status = "Pending", Role = "Initiator",  AppliedAt = now.AddHours(-3).AddMinutes(30) },
                    new() { UserId = IdPending4, Status = "Pending", Role = "Sentinel",   AppliedAt = now.AddHours(-3) },
                    new() { UserId = IdPending5, Status = "Pending", Role = "Initiator",  AppliedAt = now.AddHours(-2).AddMinutes(45) },
                    new() { UserId = IdPending6, Status = "Pending", Role = "Controller", AppliedAt = now.AddHours(-2) },
                    new() { UserId = IdPending7, Status = "Pending", Role = "Duelist",    AppliedAt = now.AddHours(-1).AddMinutes(30) },
                    new() { UserId = IdPending8, Status = "Pending", Role = "Sentinel",   AppliedAt = now.AddHours(-1) },
                },
                CreatedAt       = now.AddHours(-7),
            };

            var bulkLobbyData = new (string Id, string Title, string Game, string Desc, string HostId, string HostName, int Max, List<string> Moods, string Pic)[]
            {
                ("bbbbbbbbbbbbbbbbbbbb0001", "Valorant Ranked Push", "Valorant", "Diamond+ only, aimlab score 80k+", IdHostA, "SakuraRain", 5, new() { "Competitive", "Tryhard" }, "https://static.wikia.nocookie.net/valorant/images/8/80/Valorant_Cover_Art.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0002", "Chill Minecraft SMP", "Minecraft", "Survival server, no griefing pls", IdHostC, "PixelQueen", 10, new() { "Chill", "Casual" }, "https://minecraft.wiki/images/Minecraft_Key_Art_2024.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0003", "CS2 Faceit Grind", "CS2", "Level 8+ faceit, need serious team", IdHostB, "DarkSoulz_69", 5, new() { "Competitive" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/730/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0004", "Elden Ring Co-op NG+7", "Elden Ring", "All bosses no summon, let's suffer together", IdHostB, "DarkSoulz_69", 3, new() { "Tryhard" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/1245620/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0005", "League ARAM Night", "League of Legends", "ARAM only, no salt, just fun", IdHostA, "SakuraRain", 5, new() { "Chill", "Casual" }, "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Lux_0.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0006", "Overwatch 2 Comp Climb", "Overwatch 2", "Tank/Support duo welcome", IdHostC, "PixelQueen", 6, new() { "Competitive", "Chill" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/2357570/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0007", "Stardew Valley Co-op Farm", "Stardew Valley", "Year 1 fresh start, cozy vibes", IdHostC, "PixelQueen", 4, new() { "Chill", "Friendly" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/413150/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0008", "Dota 2 Turbo Party", "Dota 2", "Turbo mode for quick games", IdHostB, "DarkSoulz_69", 5, new() { "Casual" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/570/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0009", "Genshin Spiral Abyss Carry", "Genshin Impact", "AR60 helping AR45+ clear floor 12", IdHostA, "SakuraRain", 2, new() { "Chill", "Competitive" }, "https://genshindb.org/wp-content/uploads/2023/06/Genshin-Impact-3.6-Parade-of-Providence-Wallpaper-Desktop-1024x576.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0010", "Monster Hunter Rise Hunt", "Monster Hunter", "Afflicted investigations, MR200+", IdHostA, "SakuraRain", 4, new() { "Competitive" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/582010/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0011", "Terraria Master Mode", "Terraria", "Full playthrough from scratch", IdHostC, "PixelQueen", 4, new() { "Chill", "Tryhard" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/105600/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0012", "Among Us Custom Lobby", "Among Us", "15 players, 3 impostors, proximity chat", IdHostC, "PixelQueen", 15, new() { "Casual", "Friendly" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/945360/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0013", "FFXIV Savage Raid", "Final Fantasy XIV", "P9S-P12S prog party, know mechs", IdHostA, "SakuraRain", 8, new() { "Competitive", "Tryhard" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/39210/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0014", "Sekiro Boss Rush Race", "Sekiro", "Who dies less wins. Bragging rights only.", IdHostB, "DarkSoulz_69", 4, new() { "Tryhard", "Competitive" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/814380/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0015", "Overcooked 2 Chaos Night", "Overcooked 2", "4 star all levels or we riot", IdHostC, "PixelQueen", 4, new() { "Casual", "Chill" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/728880/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0016", "Valorant Unrated Chill", "Valorant", "No rage, just practice and fun", IdHostA, "SakuraRain", 5, new() { "Chill" }, "https://static.wikia.nocookie.net/valorant/images/8/80/Valorant_Cover_Art.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0017", "Dark Souls 3 SL1 Run", "Dark Souls 3", "Co-op SL1, no pyro allowed", IdHostB, "DarkSoulz_69", 3, new() { "Tryhard" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/374320/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0018", "League Clash Tournament", "League of Legends", "Gold+ team, need jungle & mid", IdHostA, "SakuraRain", 5, new() { "Competitive", "Tryhard" }, "https://ddragon.leagueoflegends.com/cdn/img/champion/splash/Lux_0.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0019", "CS2 Wingman Duo", "CS2", "Quick wingman games, any rank welcome", IdHostB, "DarkSoulz_69", 2, new() { "Casual" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/730/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0020", "Minecraft Modded SMP", "Minecraft", "All The Mods 9, fresh world", IdHostC, "PixelQueen", 8, new() { "Chill", "Friendly" }, "https://minecraft.wiki/images/Minecraft_Key_Art_2024.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0021", "Overwatch 2 Mystery Heroes", "Overwatch 2", "Arcade night, no tryhards pls", IdHostC, "PixelQueen", 6, new() { "Casual", "Chill" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/2357570/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0022", "Genshin Domain Farm", "Genshin Impact", "Artifact domain co-op, AR50+", IdHostA, "SakuraRain", 4, new() { "Chill" }, "https://genshindb.org/wp-content/uploads/2023/06/Genshin-Impact-3.6-Parade-of-Providence-Wallpaper-Desktop-1024x576.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0023", "Dota 2 Ranked Party", "Dota 2", "Ancient+ stack, mic required", IdHostB, "DarkSoulz_69", 5, new() { "Competitive", "Tryhard" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/570/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0024", "FFXIV Treasure Map Party", "Final Fantasy XIV", "Portal maps, bring gatherer", IdHostA, "SakuraRain", 8, new() { "Chill", "Casual" }, "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/39210/header.jpg"),
                ("bbbbbbbbbbbbbbbbbbbb0025", "Valorant 10-Man Custom", "Valorant", "In-house 5v5, balanced teams", IdHostB, "DarkSoulz_69", 10, new() { "Competitive" }, "https://static.wikia.nocookie.net/valorant/images/8/80/Valorant_Cover_Art.jpg"),
            };

            var bulkLobbies = bulkLobbyData.Select((d, i) => new Lobby
            {
                Id              = d.Id,
                Title           = d.Title,
                Game            = d.Game,
                Description     = d.Desc,
                Picture         = d.Pic,
                HostId          = d.HostId,
                HostName        = d.HostName,
                MaxPlayers      = d.Max,
                IsRecruiting    = true,
                IsComplete      = false,
                Moods           = d.Moods,
                Roles           = new() { new() { Name = "Any", Quantity = d.Max } },
                DiscordLink     = "https://discord.gg/example",
                StartRecruiting = now.AddHours(-3 - i),
                EndRecruiting   = now.AddHours(6 + i),
                StartEvent      = now.AddDays(1).AddHours(i),
                EndEvent        = now.AddDays(1).AddHours(3 + i),
                Members         = new() { new() { UserId = d.HostId, Status = "Host", Role = "Leader", AppliedAt = now.AddHours(-3 - i) } },
                CreatedAt       = now.AddHours(-4 - i),
            }).ToList();

            var lobbies = new List<Lobby> { lobby1, lobby2, lobby3, lobby4, lobby5, lobby6 };
            lobbies.AddRange(bulkLobbies);
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
