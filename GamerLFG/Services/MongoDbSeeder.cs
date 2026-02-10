using GamerLFG.Data;
using GamerLFG.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace GamerLFG.Services
{
    public class MongoDbSeeder
    {
        private readonly MongoDbContext _context;

        public MongoDbSeeder(MongoDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Check if users exist
            if (await _context.Users.CountDocumentsAsync(new BsonDocument()) > 0)
            {
                return; // Data already exists
            }

            // Seed Users
            var users = new List<User>
            {
                new User 
                { 
                    Username = "Notatord", 
                    Email = "notatord@example.com", 
                    Bio = "Casual tryhard. Mic always on.", 
                    AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=Notatord",
                    KarmaScore = 4.8,
                    VibeTags = new List<string> { "Tryhard", "Strategist" },
                    GameLibrary = new List<string> { "Valorant", "Elden Ring" },
                    IsOnline = true
                },
                new User 
                { 
                    Username = "WarriorX", 
                    Email = "warriorx@example.com", 
                    Bio = "Tank main. I protect.", 
                    AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=WarriorX",
                    KarmaScore = 4.5,
                    VibeTags = new List<string> { "Chill", "Team Player" },
                    GameLibrary = new List<string> { "Overwatch 2", "WoW" },
                    IsOnline = true
                },
                new User 
                { 
                    Username = "HealerGirl", 
                    Email = "healer@example.com", 
                    Bio = "Keeping you alive since 2010.", 
                    AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=HealerGirl",
                    KarmaScore = 5.0,
                    VibeTags = new List<string> { "Supportive", "Friendly" },
                    GameLibrary = new List<string> { "Overwatch 2", "League of Legends" },
                    IsOnline = false
                }
            };

            await _context.Users.InsertManyAsync(users);
            
            // Retrieve created users to get their IDs
            var createdUsers = await _context.Users.Find(_ => true).ToListAsync();
            var hostUser = createdUsers.FirstOrDefault(u => u.Username == "Notatord");
            var memberUser = createdUsers.FirstOrDefault(u => u.Username == "WarriorX");

            if (hostUser != null)
            {
                // Seed Lobbies
                var lobbies = new List<Lobby>
                {
                    new Lobby
                    {
                        Title = "Ranked Grind to Diamond",
                        Game = "Valorant",
                        Description = "Need a smoker and a sentinel. Must have mics.",
                        HostId = hostUser.Id,
                        MaxPlayers = 5,
                        Status = "Open",
                        IsRecruiting = true,
                        Moods = new List<string> { "Competitive", "Mic On" },
                        Roles = new List<Role>
                        {
                            new Role { Name = "Duelist", Count = 2, Filled = 1 },
                            new Role { Name = "Controller", Count = 1, Filled = 0 },
                            new Role { Name = "Sentinel", Count = 1, Filled = 0 }
                        },
                        Members = new List<Member>
                        {
                            new Member { UserId = hostUser.Id, AssignedRole = "Duelist", IsHost = true, JoinedAt = DateTime.UtcNow }
                        },
                        CreatedAt = DateTime.UtcNow
                    },
                     new Lobby
                    {
                        Title = "Chill Raid Night",
                        Game = "Destiny 2",
                        Description = "Teaching new players. Join for fun.",
                        HostId = hostUser.Id,
                        MaxPlayers = 6,
                        Status = "Open",
                        IsRecruiting = true,
                        Moods = new List<string> { "Chill", "Sherpa" },
                         Roles = new List<Role>
                        {
                            new Role { Name = "Any", Count = 5, Filled = 1 }
                        },
                        Members = new List<Member>
                        {
                            new Member { UserId = hostUser.Id, AssignedRole = "Sherpa", IsHost = true, JoinedAt = DateTime.UtcNow },
                             new Member { UserId = memberUser?.Id ?? "", AssignedRole = "Learner", IsHost = false, JoinedAt = DateTime.UtcNow }
                        },
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    }
                };

                await _context.Lobbies.InsertManyAsync(lobbies);
            }
        }
    }
}
