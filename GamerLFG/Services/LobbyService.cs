using GamerLFG.Models;
using GamerLFG.Models.ViewModels;
using GamerLFG.service;
using GamerLFG.Services.Interface;
using GamerLFG.Services.Interface.DTOs;
using MongoDB.Driver;
namespace GamerLFG.Services
{
    public class LobbyService : ILobbyService
    {   
         private readonly MongoDBservice _database;
        public LobbyService(MongoDBservice database)
        {
            _database = database;
            
        }
     
        // _database.Users;
        public async Task<LobbyListResponse> GetAllLobbyAsync(string? userId = null){ // ใช้function แบบ async

            var allLobbies = await _database.Lobbies.Find(lobby => true).SortBy(l => l.Id).ToListAsync();

            // batch-fetch host users
            var hostIds = allLobbies.Select(l => l.HostId).Where(id => id != null).Distinct().ToList();
            var hostUsers = hostIds.Any()
                ? await _database.Users.Find(u => hostIds.Contains(u.Id)).ToListAsync()
                : new List<User>();
            var hostMap = hostUsers.ToDictionary(u => u.Id);

            // Filter ใน C# แทนใน MongoDB query เพื่อหลีกเลี่ยง type mismatch
            var myLobbyList = string.IsNullOrEmpty(userId)
                ? new List<Lobby>()
                : allLobbies.Where(lob => lob.HostId == userId).ToList();

            var otherLobbyList = string.IsNullOrEmpty(userId)
                ? allLobbies.Take(10).ToList()
                : allLobbies.Where(lob => lob.HostId != userId).Take(10).ToList();

            var myLobby = myLobbyList.Select(lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = hostMap.TryGetValue(lob.HostId ?? "", out var hu1) ? hu1.Username : lob.HostId,
                Picture = lob.Picture,
                Moods = lob.Moods,
                Currentplayers = lob.Members?.Count ?? 0,
                MaxPlayers = lob.MaxPlayers
            }).ToList();
            var publicLobby = otherLobbyList.Select(lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = hostMap.TryGetValue(lob.HostId ?? "", out var hu2) ? hu2.Username : lob.HostId,
                Picture = lob.Picture,
                Moods = lob.Moods,
                Currentplayers = lob.Members?.Count ?? 0,
                MaxPlayers = lob.MaxPlayers
            }).ToList();

            return new LobbyListResponse
            {
                MyLobbies = myLobby,
                OtherLobbies = publicLobby
            };

            }
            
            
        public async Task<List<ShowLobbyDTO>> GetNextLobbiesAsync(string? lastId,string? userId, int pageSize = 10)
            {
                // ใช้ .Ne() แทน .Eq()
                var notYours = Builders<Lobby>.Filter.Ne(l => l.HostId, userId);

                
                // กรองเอาเฉพาะตัวที่ ID "หลังจาก" ตัวสุดท้ายที่หน้าจอแสดงอยู่
                var filter = Builders<Lobby>.Filter.Gt(l => l.Id, lastId);
                var nextLobby = await _database.Lobbies.Find(filter & notYours)
                                .SortBy(l => l.Id)
                                .Limit(pageSize)
                                .ToListAsync();

                var nextHostIds = nextLobby.Select(l => l.HostId).Where(id => id != null).Distinct().ToList();
                var nextHostUsers = nextHostIds.Any()
                    ? await _database.Users.Find(u => nextHostIds.Contains(u.Id)).ToListAsync()
                    : new List<User>();
                var nextHostMap = nextHostUsers.ToDictionary(u => u.Id);

                return nextLobby.Select(lob => new ShowLobbyDTO{
                    Id = lob.Id,
                    Title  = lob.Title,
                    Game = lob.Game,
                    Description = lob.Description,
                    HostName  = nextHostMap.TryGetValue(lob.HostId ?? "", out var hu) ? hu.Username : lob.HostId,
                    Picture = lob.Picture,
                    Moods = lob.Moods,
                    Currentplayers = lob.Members.Count,
                    MaxPlayers = lob.MaxPlayers
                    }).ToList();
                }

        public async Task<(bool success, string message)> CreateLobbyAsync(CreateLobbyDTO newLobby) 
        {
            try {
                var lobby = newLobby.ToEntity();
                
                // Debug ดูว่า Entity พร้อมใช้งานไหม
                Console.WriteLine($"Inserting Lobby: {System.Text.Json.JsonSerializer.Serialize(lobby)}");

                await _database.Lobbies.InsertOneAsync(lobby);
                
                // ถ้ามาถึงตรงนี้ได้ แสดงว่าคำสั่งส่งออกไปสำเร็จ
                return (true, "OK");
            }
            catch (MongoException ex) {
                // ดัก Error เฉพาะจาก MongoDB
                Console.WriteLine($"MongoDB Error: {ex.Message}");
                return (false, ex.Message);
            }
            catch (Exception ex) {
                // ดัก Error อื่นๆ เช่น Mapping พัง
                Console.WriteLine($"General Error: {ex.Message}");
                return (false, ex.Message);
            }
        }
                // public async Task<(bool success,string message)> CreateLobbyAsync(CreateLobbyDTO newLobby){
        //     var lobby = newLobby.ToEntity();
        //     Console.Write(newLobby);
        //     // var hostObj = await _users.Find(u => u.Id == lobby.HostId).FirstOrDefaultAsync();
        //     // if (hostObj == null)
        //     //     {
        //     //     return (false,"Host not found");
        //     //     }
        //     // string hostName = hostObj.Username;
        //     await _database.Lobbies.InsertOneAsync(lobby);
        //     return (true,"OK");
        // }
        public async Task<bool> DeleteLobbyAsync(string id)
        {
            var result = await _database.Lobbies.DeleteOneAsync(l => l.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task UpdateLobbyAsync(Lobby lobby)
        {
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobby.Id);
            await _database.Lobbies.ReplaceOneAsync(filter, lobby);
        }

        public async Task AddmemberAsync(Lobby current_lobby, User newUser) { }

        public async Task<Lobby?> GetLobbyByIdAsync(string id)
        {
            return await _database.Lobbies.Find(l => l.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> ApplyToLobbyAsync(string lobbyId, string userId, string role)
        {
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.Push(l => l.Members, new LobbyMember
            {
                UserId = userId,
                Status = "Pending",
                Role = role
            });
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> CancelApplicationAsync(string lobbyId, string userId)
        {
            var filter = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId),
                Builders<Lobby>.Filter.ElemMatch(l => l.Members, m => m.UserId == userId && m.Status == "Pending")
            );
            var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId && m.Status == "Pending");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RecruitMemberAsync(string lobbyId, string userId)
        {
            var filter = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId),
                Builders<Lobby>.Filter.ElemMatch(l => l.Members, m => m.UserId == userId && m.Status == "Pending")
            );
            var update = Builders<Lobby>.Update.Set("Members.$.Status", "joined");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> RejectApplicantAsync(string lobbyId, string userId)
        {
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId && m.Status == "Pending");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> KickMemberAsync(string lobbyId, string userId)
        {
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId);
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> CompleteLobbyAsync(string lobbyId)
        {
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.Set(l => l.IsComplete, true)
                                               .Set(l => l.IsRecruiting, false);
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> SubmitKarmaAsync(string fromUserId, string targetUserId, double score)
        {
            var karmaHistory = new KarmaHistory
            {
                FromUserId = fromUserId,
                TargetUserId = targetUserId,
                Score = score,
                Date = DateTime.UtcNow
            };
            await _database.KarmaHistories.InsertOneAsync(karmaHistory);

            var allHistories = await _database.KarmaHistories
                .Find(k => k.TargetUserId == targetUserId)
                .ToListAsync();

            if (allHistories.Any())
            {
                var averageScore = allHistories.Average(k => k.Score);
                var userFilter = Builders<User>.Filter.Eq(u => u.Id, targetUserId);
                var userUpdate = Builders<User>.Update.Set(u => u.KarmaScore, averageScore);
                await _database.Users.UpdateOneAsync(userFilter, userUpdate);
            }

            return true;
        }

        public async Task<LobbyDetailsViewModel?> GetLobbyDetailsAsync(string id, string? currentUserId)
        {
            // 1. ดึง lobby จาก DB
            var lobby = await _database.Lobbies
                .Find(l => l.Id == id)
                .FirstOrDefaultAsync();

            if (lobby == null) return null;

            // 2. ดึง user ของทุก member (Status != "Pending")
            var memberIds = lobby.Members
                .Where(m => m.Status != "Pending")
                .Select(m => m.UserId)
                .ToList();

            var memberUsers = memberIds.Any()
                ? await _database.Users.Find(u => memberIds.Contains(u.Id)).ToListAsync()
                : new List<User>();

            var memberMap = memberUsers.ToDictionary(u => u.Id);

            // 3. กำหนด role flags
            bool isHost   = currentUserId == lobby.HostId;
            bool isMember = lobby.Members.Any(m =>
                m.UserId == currentUserId && m.Status != "Pending");

            // 4. Pending members (มองเห็นเฉพาะ Host) + hasPendingRequest (สำหรับ Visitor)
            var pendingApps  = new List<LobbyMember>();
            var applicantMap = new Dictionary<string, User>();
            bool hasPendingRequest = false;

            if (isHost)
            {
                pendingApps = lobby.Members
                    .Where(m => m.Status == "Pending")
                    .ToList();

                var applicantIds = pendingApps
                    .Select(m => m.UserId)
                    .Where(uid => uid != null)
                    .ToList();

                if (applicantIds.Any())
                {
                    var applicants = await _database.Users
                        .Find(u => applicantIds.Contains(u.Id))
                        .ToListAsync();
                    applicantMap = applicants.ToDictionary(u => u.Id);
                }
            }
            else if (!string.IsNullOrEmpty(currentUserId))
            {
                hasPendingRequest = lobby.Members
                    .Any(m => m.UserId == currentUserId && m.Status == "Pending");
            }

            // 5. Karma — ดึง user ที่ currentUser rate ไปแล้วในกลุ่มนี้
            var endorsedUserIds = new HashSet<string>();
            if (lobby.IsComplete && !string.IsNullOrEmpty(currentUserId))
            {
                var karmaGiven = await _database.KarmaHistories
                    .Find(k => k.FromUserId == currentUserId &&
                               memberIds.Contains(k.TargetUserId))
                    .ToListAsync();
                endorsedUserIds = karmaGiven.Select(k => k.TargetUserId).ToHashSet();
            }

            // 6. ดึง CurrentUser — ถ้าไม่อยู่ใน MemberMap (visitor/pending) ให้ fetch เพิ่ม
            User? currentUser = null;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                currentUser = memberMap.TryGetValue(currentUserId, out var cu)
                    ? cu
                    : await _database.Users.Find(u => u.Id == currentUserId).FirstOrDefaultAsync();
            }

            return new LobbyDetailsViewModel
            {
                Lobby               = lobby,
                CurrentUserId       = currentUserId,
                CurrentUser         = currentUser,
                IsHost              = isHost,
                IsMember            = isMember || isHost,
                HasPendingRequest   = hasPendingRequest,
                MemberMap           = memberMap,
                PendingApplications = pendingApps,
                ApplicantMap        = applicantMap,
                EndorsedUserIds     = endorsedUserIds,
            };
        }
    }
}

