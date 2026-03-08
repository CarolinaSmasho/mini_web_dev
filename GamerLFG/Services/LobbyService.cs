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
        // _database.Users;
        public async Task<LobbyListResponse> GetAllLobbyAsync(string? userId = null){ // ใช้function แบบ async
            
            var myLobbyList = await _database.Lobbies.Find(mylobby => mylobby.HostId == userId).ToListAsync(); // ดึง lobby ของเรามาแบบ async
            var otherLobbyList = await _database.Lobbies.Find(otherLobby => otherLobby.HostId != userId).SortBy(l => l.Id).Limit(10).ToListAsync();
            var myLobby = myLobbyList.Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = lob.HostName,
                Picture = lob.Picture,
                Moods = lob.Moods,
                CurrentPlayers = lob.Members.Count,
                MaxPlayers = lob.MaxPlayers
                
            }).ToList();
            var publicLobby = otherLobbyList.Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = lob.HostName,
                Picture = lob.Picture,
                Moods = lob.Moods,
                CurrentPlayers = lob.Members.Count,
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
                    CurrentPlayers = lob.Members.Count,
                    MaxPlayers = lob.MaxPlayers
                    }).ToList();
                }

        public async Task<(bool success, string message)> CreateLobbyAsync(CreateLobbyDTO newLobby) 
        {
            try {
                var timeValid = newLobby.TimeValidation();
                if (!timeValid.valid)
                {

                    return (false,timeValid.erMessage);
                }
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
            // Fetch the lobby first to check capacity
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;

            // Find the role slot definition
            var roleDef = lobby.Roles.FirstOrDefault(r => r.Name == role);
            if (roleDef != null)
            {
                // Count how many members (pending + joined) already hold this role
                var takenCount = lobby.Members.Count(m =>
                    m.Role == role && (m.Status == "Pending" || m.Status == "joined"));

                if (takenCount >= roleDef.Quantity)
                    return false; // role is full
            }

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.Push(l => l.Members, new LobbyMember
            {
                UserId = userId,
                Status = "Pending",
                Role = role
            });
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            //noti ahh
            if (result.ModifiedCount > 0)
            {
                var applicant = await _database.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                var applicantName = applicant != null ? applicant.Username : "have people";
                var notification = new Notification
                {
                    Type = "lobby_request_apply",
                    RelateObjectId = lobbyId,
                    UserId = lobby.HostId, 
                    Text = $"{applicantName} ได้ส่งคำขอเข้าร่วมห้อง {lobby.Title} ของคุณในตำแหน่ง {role}",
                    IsRead = false,
                    Date = DateTime.UtcNow
                };
                await _database.Notifications.InsertOneAsync(notification);
            }
            
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
        {   //lobby title
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;

            var filter = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId),
                Builders<Lobby>.Filter.ElemMatch(l => l.Members, m => m.UserId == userId && m.Status == "Pending")
            );
            var update = Builders<Lobby>.Update.Set("Members.$.Status", "joined");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
             //noti ahh
            if (result.ModifiedCount > 0)
            {
                var notification = new Notification
                {
                    Type = "lobby_accepted", 
                    RelateObjectId = lobbyId, 
                    UserId = userId, 
                    Text = $"คำขอเข้าร่วมห้อง {lobby.Title} ของคุณได้รับการอนุมัติแล้ว",
                    IsRead = false,
                    Date = DateTime.UtcNow
                };

                // บันทึกลงตาราง Notifications
                await _database.Notifications.InsertOneAsync(notification);
            }
            return result.ModifiedCount > 0;
            
        }

        public async Task<bool> RejectApplicantAsync(string lobbyId, string userId)
        {   
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;
                
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId && m.Status == "Pending");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
             //noti ahh
            if (result.ModifiedCount > 0)
            {
                var notification = new Notification
                {
                    Type = "lobby_rejected",
                    RelateObjectId = lobbyId,
                    UserId = userId, 
                    Text = $"คำขอเข้าร่วมห้อง {lobby.Title} ของคุณถูกปฏิเสธ",
                    IsRead = false,
                    Date = DateTime.UtcNow
                };
                await _database.Notifications.InsertOneAsync(notification);
            }
            return result.ModifiedCount > 0;
        }

        public async Task<bool> KickMemberAsync(string lobbyId, string userId)
        {   
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId);
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);

            //noti ahh
            if (result.ModifiedCount > 0)
            {
                var notification = new Notification
                {
                    Type = "lobby_kicked",
                    RelateObjectId = lobbyId,
                    UserId = userId, 
                    Text = $"คุณถูกเตะออกจากห้อง {lobby.Title}",
                    IsRead = false,
                    Date = DateTime.UtcNow
                };
                await _database.Notifications.InsertOneAsync(notification);
            }
            return result.ModifiedCount > 0;


        }

        public async Task<bool> CompleteLobbyAsync(string lobbyId)
        {   
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.Set(l => l.IsComplete, true)
                                               .Set(l => l.IsRecruiting, false);
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            //noti ahh
            if (result.ModifiedCount > 0)
            {
                var notifications = new List<Notification>();

                
                var activeMembers = lobby.Members
                                        .Where(m => m.Status == "joined")
                                        .Select(m => m.UserId)
                                        .ToList();

                
                if (!activeMembers.Contains(lobby.HostId))
                {
                    activeMembers.Add(lobby.HostId);
                }

                foreach (var memberId in activeMembers)
                {
                    notifications.Add(new Notification
                    {
                        Type = "lobby_end", 
                        RelateObjectId = lobbyId,
                        UserId = memberId, 
                        Text = $"ห้อง {lobby.Title} จบลงแล้ว อย่าลืมเข้าไปโหวตให้คะแนน Karma ให้เพื่อนร่วมทีมด้วยจ๊ะนะ",
                        IsRead = false,
                        Date = DateTime.UtcNow
                    });
                }

                
                if (notifications.Any())
                {
                    await _database.Notifications.InsertManyAsync(notifications);
                }
            }
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

            //noti ahh
            var sender = await _database.Users.Find(u => u.Id == fromUserId).FirstOrDefaultAsync();
            var senderName = sender != null ? sender.Username : "เพื่อนร่วมทีม";
            var notification = new Notification
            {
                Type = "karma_received", 
                RelateObjectId = fromUserId, 
                UserId = targetUserId, 
                Text = $"{senderName} ได้โหวตคะแนน Karma ให้คุณ {score} คะแนน",
                IsRead = false,
                Date = DateTime.UtcNow
            };
            await _database.Notifications.InsertOneAsync(notification);
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

