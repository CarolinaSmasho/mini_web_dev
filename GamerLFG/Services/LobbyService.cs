using GamerLFG.Models;
using GamerLFG.Models.ViewModels;
using GamerLFG.service;
using GamerLFG.Services.Interface;
using GamerLFG.Services.Interface.DTOs;
using MongoDB.Driver;
using MongoDB.Bson;
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
            
            var now = DateTime.UtcNow;
            var isRecruiting = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Gte(l => l.EndRecruiting, now),
                Builders<Lobby>.Filter.Lte(l => l.StartRecruiting, now),
                Builders<Lobby>.Filter.Eq(l => l.IsRecruiting, true),
                Builders<Lobby>.Filter.Eq(l => l.IsComplete, false)
            );
            // var isRecruiting =  Builders<Lobby>.Filter.Eq(l =>l.GetStatus(), LobbyStatus.Recruiting);
            var isMine = Builders<Lobby>.Filter.Eq(l => l.HostId,userId);
            var notMine = Builders<Lobby>.Filter.Ne(l => l.HostId,userId);
            var myLobbyList = await _database.Lobbies.Find(isMine).ToListAsync(); // ดึง lobby ของเรามาแบบ async
            var otherLobbyList = await _database.Lobbies.Find(notMine & isRecruiting).SortBy(l => l.Id).Limit(10).ToListAsync();
            var myLobby = myLobbyList.Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = lob.HostName,
                Picture = lob.Picture,
                Moods = lob.Moods,
                CurrentPlayers = lob.Members.Count,
                MaxPlayers = lob.MaxPlayers,
                Status = lob.GetStatus(),
                isRecuiting = lob.IsRecruiting
                
            }).ToList();
            var otherHostIds = otherLobbyList.Select(l => l.HostId).Where(id => id != null).Distinct().ToList();
                var nextHostUsers = otherHostIds.Any()
                    ? await _database.Users.Find(u => otherHostIds.Contains(u.Id)).ToListAsync()
                    : new List<User>();
                var otherHostMap = nextHostUsers.ToDictionary(u => u.Id);


            var publicLobby = otherLobbyList.Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = otherHostMap.TryGetValue(lob.HostId ?? "", out var hu) ? hu.Username : lob.HostId,
                Picture = lob.Picture,
                Moods = lob.Moods,
                CurrentPlayers = lob.Members.Count,
                MaxPlayers = lob.MaxPlayers,
                Status = lob.GetStatus(),
                isRecuiting = lob.IsRecruiting
                
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
                var now = DateTime.UtcNow;
                var isRecruiting = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Gte(l => l.EndRecruiting, now),
                Builders<Lobby>.Filter.Lte(l => l.StartRecruiting, now),
                Builders<Lobby>.Filter.Eq(l => l.IsRecruiting, true),
                Builders<Lobby>.Filter.Eq(l => l.IsComplete, false)
                 );
                
                // กรองเอาเฉพาะตัวที่ ID "หลังจาก" ตัวสุดท้ายที่หน้าจอแสดงอยู่
                var filter = Builders<Lobby>.Filter.Gt(l => l.Id, lastId);
                var nextLobby = await _database.Lobbies.Find(filter & notYours & isRecruiting)
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
                    MaxPlayers = lob.MaxPlayers,
                    Status = lob.GetStatus(),
                    isRecuiting = lob.IsRecruiting
                    }).ToList();
                }

        //

        public async Task<(bool success, string message)> CreateLobbyAsync(CreateLobbyDTO newLobby) 
        {
            try {
                newLobby.StartRecruiting = DateTime.SpecifyKind(newLobby.StartRecruiting, DateTimeKind.Local).ToUniversalTime();
                newLobby.EndRecruiting = DateTime.SpecifyKind(newLobby.EndRecruiting, DateTimeKind.Local).ToUniversalTime();
                newLobby.StartEvent = DateTime.SpecifyKind(newLobby.StartEvent, DateTimeKind.Local).ToUniversalTime();
                newLobby.EndEvent = DateTime.SpecifyKind(newLobby.EndEvent, DateTimeKind.Local).ToUniversalTime();
                
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
            lobby.StartRecruiting = DateTime.SpecifyKind(lobby.StartRecruiting, DateTimeKind.Local).ToUniversalTime();
            lobby.EndRecruiting = DateTime.SpecifyKind(lobby.EndRecruiting, DateTimeKind.Local).ToUniversalTime();
            lobby.StartEvent = DateTime.SpecifyKind(lobby.StartEvent, DateTimeKind.Local).ToUniversalTime();
            lobby.EndEvent = DateTime.SpecifyKind(lobby.EndEvent, DateTimeKind.Local).ToUniversalTime();
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobby.Id);
            await _database.Lobbies.ReplaceOneAsync(filter, lobby);
        }

        public async Task AddmemberAsync(Lobby current_lobby, User newUser) { }

        public async Task<Lobby?> GetLobbyByIdAsync(string id)
        {
            return await _database.Lobbies.Find(l => l.Id == id).FirstOrDefaultAsync();
        }

        public async Task<(bool success, string message)> ApplyToLobbyAsync(string lobbyId, string userId, string role)
        {
            // Fetch the lobby first to check capacity
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return (false, "Lobby not found.");

            // ตรวจสอบสถานะรวมเวลา: ต้องอยู่ในช่วง Recruiting เท่านั้น
            var status = lobby.GetStatus();
            if (status != LobbyStatus.Recruiting)
            {
                var reason = status switch
                {
                    LobbyStatus.ComingSoon   => "Recruitment has not started yet.",
                    LobbyStatus.EventOngoing => "Recruitment is closed — the event has already started.",
                    LobbyStatus.Completed    => "This lobby has been completed.",
                    LobbyStatus.Cancelled    => "Recruitment is closed.",
                    _                        => "Recruitment is currently closed."
                };
                return (false, reason);
            }

            // Find the role slot definition
            var roleDef = lobby.Roles.FirstOrDefault(r => r.Name == role);
            if (roleDef != null)
            {
                // Count ALL members holding this role (Host + joined + Pending)
                var takenCount = lobby.Members.Count(m => m.Role == role);

                if (takenCount >= roleDef.Quantity)
                    return (false, "This role is already full.");
            }

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.Push(l => l.Members, new LobbyMember
            {
                UserId = userId,
                Status = "Pending",
                Role = role,
                AppliedAt = DateTime.UtcNow
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
                    Date = DateTime.Now
                };
                await _database.Notifications.InsertOneAsync(notification);
            }
            
            return result.ModifiedCount > 0
                ? (true, "OK")
                : (false, "Failed to submit request.");
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
                    Date = DateTime.Now
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
                    Date = DateTime.Now
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
                    Date = DateTime.Now
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
                        Date = DateTime.Now
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
                Date = DateTime.Now
            };
            await _database.Notifications.InsertOneAsync(notification);
            return true;
        }

        public async Task<bool> ChangeMemberRoleAsync(string lobbyId, string userId, string newRole)
        {
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;

            var member = lobby.Members.FirstOrDefault(m => m.UserId == userId && m.Status != "Pending");
            if (member == null) return false;

            // "Other" is always allowed without slot limits
            if (newRole != "Other")
            {
                var roleDef = lobby.Roles.FirstOrDefault(r => r.Name == newRole);
                if (roleDef == null) return false;

                var takenCount = lobby.Members.Count(m =>
                    m.Role == newRole && m.UserId != userId && m.Status != "Pending");

                if (takenCount >= roleDef.Quantity)
                    return false; // role is full
            }

            // Update the member's role
            var filter = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId),
                Builders<Lobby>.Filter.ElemMatch(l => l.Members, m => m.UserId == userId && m.Status != "Pending")
            );
            var update = Builders<Lobby>.Update.Set("Members.$.Role", newRole);
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ToggleRecruitmentAsync(string lobbyId)
        {
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.Set(l => l.IsRecruiting, !lobby.IsRecruiting);
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<LobbyDetailsViewModel?> GetLobbyDetailsAsync(string id, string? currentUserId)
        {
            // 1. ดึง lobby จาก DB
            var lobby = await _database.Lobbies
                .Find(l => l.Id == id)
                .FirstOrDefaultAsync();

            if (lobby == null) return null;

            // 2. ดึง user ของทุก member (Status != "Pending" && != "Invited")
            var memberIds = lobby.Members
                .Where(m => m.Status != "Pending" && m.Status != "Invited")
                .Select(m => m.UserId)
                .ToList();
            
            var memberUsers = memberIds.Any()
                ? await _database.Users.Find(u => memberIds.Contains(u.Id)).ToListAsync()
                : new List<User>();

            var memberMap = memberUsers.ToDictionary(u => u.Id);

            // 3. กำหนด role flags
            bool isHost   = currentUserId == lobby.HostId;
            bool isMember = lobby.Members.Any(m =>
                m.UserId == currentUserId && m.Status != "Pending" && m.Status != "Invited");

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

            // 7. Friend invite data
            var invitableFriends = new List<User>();
            bool hasPendingInvite = false;
            string? invitedByName = null;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                // Check if currentUser has a pending invite (Status == "Invited")
                var inviteMember = lobby.Members.FirstOrDefault(m => m.UserId == currentUserId && m.Status == "Invited");
                if (inviteMember != null)
                {
                    hasPendingInvite = true;
                    if (inviteMember.InvitedBy != null)
                    {
                        var inviter = memberMap.TryGetValue(inviteMember.InvitedBy, out var inv)
                            ? inv
                            : await _database.Users.Find(u => u.Id == inviteMember.InvitedBy).FirstOrDefaultAsync();
                        invitedByName = inviter?.Username ?? "someone";
                    }
                }

                // Populate invitable friends for members when lobby is recruiting
                if ((isMember || isHost) && lobby.GetStatus() == LobbyStatus.Recruiting && currentUser != null)
                {
                    var allMemberUserIds = lobby.Members.Select(m => m.UserId).ToHashSet();
                    var invitableFriendIds = currentUser.FriendIds
                        .Where(fid => !allMemberUserIds.Contains(fid))
                        .ToList();

                    if (invitableFriendIds.Any())
                    {
                        invitableFriends = await _database.Users
                            .Find(u => invitableFriendIds.Contains(u.Id))
                            .ToListAsync();
                    }
                }
            }

            return new LobbyDetailsViewModel
            {
                Lobby               = lobby,
                CurrentUserId       = currentUserId,
                CurrentUser         = currentUser,
                IsHost              = isHost,
                IsMember            = isMember || isHost,
                HasPendingRequest   = hasPendingRequest,
                HasPendingInvite    = hasPendingInvite,
                InvitedByName       = invitedByName,
                InvitableFriends    = invitableFriends,
                MemberMap           = memberMap,
                PendingApplications = pendingApps,
                ApplicantMap        = applicantMap,
                EndorsedUserIds     = endorsedUserIds,
            };
        }

        
        public async Task ProcessAutoRecruitAsync(string lobbyId)
        {
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null || lobby.IsComplete || lobby.AutoRecruitProcessed) return;

            var pendingMembers = lobby.Members.Where(m => m.Status == "Pending").ToList();
            if (!pendingMembers.Any())
            {
                // No pending members, just mark as processed
                var markFilter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
                var markUpdate = Builders<Lobby>.Update
                    .Set(l => l.AutoRecruitProcessed, true)
                    .Set(l => l.IsRecruiting, false);
                await _database.Lobbies.UpdateOneAsync(markFilter, markUpdate);
                return;
            }

            // Fetch user objects to get KarmaScore
            var pendingUserIds = pendingMembers.Select(m => m.UserId).ToList();
            var pendingUsers = await _database.Users.Find(u => pendingUserIds.Contains(u.Id)).ToListAsync();
            var userKarmaMap = pendingUsers.ToDictionary(u => u.Id, u => u.KarmaScore);

            // Sort: KarmaScore DESC, then AppliedAt ASC (earliest first for ties)
            var sorted = pendingMembers
                .OrderByDescending(m => userKarmaMap.GetValueOrDefault(m.UserId, 0))
                .ThenBy(m => m.AppliedAt)
                .ToList();

            // Calculate available slots
            var currentJoined = lobby.Members.Count(m => m.Status != "Pending" && m.Status != "Invited");
            var availableSlots = Math.Max(0, lobby.MaxPlayers - currentJoined);

            var accepted = sorted.Take(availableSlots).ToList();
            var rejected = sorted.Skip(availableSlots).ToList();

            // Update lobby in memory
            foreach (var member in accepted)
            {
                member.Status = "joined";
                member.Role = "Other";
            }

            // Remove rejected members
            foreach (var member in rejected)
            {
                lobby.Members.Remove(member);
            }

            lobby.AutoRecruitProcessed = true;
            lobby.IsRecruiting = false;

            // Save all changes at once
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            await _database.Lobbies.ReplaceOneAsync(filter, lobby);

            // Send notifications
            foreach (var member in accepted)
            {
                await _database.Notifications.InsertOneAsync(new Notification
                {
                    Type = "lobby_accepted",
                    RelateObjectId = lobbyId,
                    UserId = member.UserId,
                    Text = $"คุณได้รับการอนุมัติเข้าร่วมห้อง {lobby.Title} โดยอัตโนมัติ (ตำแหน่ง Other)",
                    IsRead = false,
                    Date = DateTime.UtcNow
                });
            }

            foreach (var member in rejected)
            {
                await _database.Notifications.InsertOneAsync(new Notification
                {
                    Type = "lobby_rejected",
                    RelateObjectId = lobbyId,
                    UserId = member.UserId,
                    Text = $"คำขอเข้าร่วมห้อง {lobby.Title} ของคุณถูกปฏิเสธ (ห้องเต็มแล้ว)",
                    IsRead = false,
                    Date = DateTime.UtcNow
                });
            }
        }

        public async Task<(bool success, string message)> InviteFriendAsync(string lobbyId, string inviterId, string friendId, string role)
        {
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return (false, "Lobby not found.");

            var status = lobby.GetStatus();
            if (status != LobbyStatus.Recruiting)
                return (false, "Recruitment is currently closed.");

            // Verify inviter is a member
            var inviterMember = lobby.Members.FirstOrDefault(m => m.UserId == inviterId && (m.Status == "joined" || m.Status == "Host"));
            if (inviterMember == null)
                return (false, "You must be a member to invite friends.");

            // Verify friend is not already in the lobby
            if (lobby.Members.Any(m => m.UserId == friendId))
                return (false, "This user is already in the lobby.");

            // Verify friend is in inviter's FriendIds
            var inviterUser = await _database.Users.Find(u => u.Id == inviterId).FirstOrDefaultAsync();
            if (inviterUser == null || !inviterUser.FriendIds.Contains(friendId))
                return (false, "You can only invite your friends.");

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.Push(l => l.Members, new LobbyMember
            {
                UserId = friendId,
                Status = "Invited",
                Role = role,
                InvitedBy = inviterId,
                AppliedAt = DateTime.UtcNow
            });
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                var friendUser = await _database.Users.Find(u => u.Id == friendId).FirstOrDefaultAsync();
                var friendName = friendUser?.Username ?? "Someone";

                // Send notification to friend
                await _database.Notifications.InsertOneAsync(new Notification
                {
                    Type = "lobby_invite",
                    RelateObjectId = lobbyId,
                    UserId = friendId,
                    Text = $"{inviterUser.Username} ชวนคุณเข้าร่วมห้อง {lobby.Title}",
                    IsRead = false,
                    Date = DateTime.UtcNow
                });
            }

            return result.ModifiedCount > 0
                ? (true, "OK")
                : (false, "Failed to send invite.");
        }

        public async Task<(bool success, string message)> AcceptInviteAsync(string lobbyId, string userId)
        {
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return (false, "Lobby not found.");

            var member = lobby.Members.FirstOrDefault(m => m.UserId == userId && m.Status == "Invited");
            if (member == null) return (false, "No pending invite found.");

            // Change status from "Invited" to "Pending" (waiting for host approval)
            var filter = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId),
                Builders<Lobby>.Filter.ElemMatch(l => l.Members, m => m.UserId == userId && m.Status == "Invited")
            );
            var update = Builders<Lobby>.Update.Set("Members.$.Status", "Pending");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                var friendUser = await _database.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                var inviterUser = await _database.Users.Find(u => u.Id == member.InvitedBy).FirstOrDefaultAsync();
                var friendName = friendUser?.Username ?? "Someone";
                var inviterName = inviterUser?.Username ?? "someone";

                // Send notification to host
                await _database.Notifications.InsertOneAsync(new Notification
                {
                    Type = "lobby_invite_request",
                    RelateObjectId = lobbyId,
                    UserId = lobby.HostId,
                    Text = $"{friendName} ตอบรับคำเชิญของ {inviterName} และรอการอนุมัติเข้าร่วมห้อง {lobby.Title}",
                    IsRead = false,
                    Date = DateTime.UtcNow
                });
            }

            return result.ModifiedCount > 0
                ? (true, "OK")
                : (false, "Failed to accept invite.");
        }

        public async Task<(bool success, string message)> DeclineInviteAsync(string lobbyId, string userId)
        {
            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return (false, "Lobby not found.");

            var member = lobby.Members.FirstOrDefault(m => m.UserId == userId && m.Status == "Invited");
            if (member == null) return (false, "No pending invite found.");

            // Remove the invited member
            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId && m.Status == "Invited");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0 && member.InvitedBy != null)
            {
                var friendUser = await _database.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                var friendName = friendUser?.Username ?? "Someone";

                // Notify inviter that friend declined
                await _database.Notifications.InsertOneAsync(new Notification
                {
                    Type = "lobby_invite_declined",
                    RelateObjectId = lobbyId,
                    UserId = member.InvitedBy,
                    Text = $"{friendName} ปฏิเสธคำเชิญเข้าร่วมห้อง {lobby.Title}",
                    IsRead = false,
                    Date = DateTime.UtcNow
                });
            }

            return result.ModifiedCount > 0
                ? (true, "OK")
                : (false, "Failed to decline invite.");
        }

        public async Task<List<ShowLobbyDTO>> GetLobbiesAsyncByName(string? lobName,string? userId,string userName = "", int pageSize = 9000)
        {
            
                var builder = Builders<Lobby>.Filter;
                var notYours = builder.Ne(l => l.HostId, userId);
                var filter = builder.Regex(x => x.Title, new BsonRegularExpression(lobName, "i"));
                var findByUserName = builder.Eq(l => l.HostName,userName);
                var now = DateTime.UtcNow;
                var isRecruiting = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Gte(l => l.EndRecruiting, now),
                Builders<Lobby>.Filter.Lte(l => l.StartRecruiting, now),
                Builders<Lobby>.Filter.Eq(l => l.IsRecruiting, true),
                Builders<Lobby>.Filter.Eq(l => l.IsComplete, false)
                );
                List<Lobby> nextLobby = new();
                if (userName == "")
                {
                    nextLobby = await _database.Lobbies.Find(filter & notYours)
                                    .SortBy(l => l.Id)
                                    .Limit(pageSize)
                                    .ToListAsync();
                }
                else
                {
                    nextLobby = await _database.Lobbies.Find(findByUserName & isRecruiting)
                                    .SortBy(l => l.Id)
                                    .Limit(pageSize)
                                    .ToListAsync();
                }    

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
                    MaxPlayers = lob.MaxPlayers,
                    isRecuiting = lob.IsRecruiting,
                    Status = lob.GetStatus()

                    }).ToList();
        }
    }

    
       

}