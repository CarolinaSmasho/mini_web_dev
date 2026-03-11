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
     

        public async Task<LobbyListResponse> GetAllLobbyAsync(string? userId = null){
            
            var now = DateTime.UtcNow;
            var isRecruiting = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Gte(l => l.EndRecruiting, now),
                Builders<Lobby>.Filter.Lte(l => l.StartRecruiting, now),
                Builders<Lobby>.Filter.Eq(l => l.IsRecruiting, true),
                Builders<Lobby>.Filter.Eq(l => l.IsComplete, false)
            );

            var isMine = Builders<Lobby>.Filter.Eq(l => l.HostId,userId);
            var notMine = Builders<Lobby>.Filter.Ne(l => l.HostId,userId);
            var myLobbyList = await _database.Lobbies.Find(isMine).ToListAsync();
            var otherLobbyList = await _database.Lobbies.Find(notMine & isRecruiting).SortBy(l => l.Id).Limit(10).ToListAsync();
             
            var myLobby = myLobbyList.Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = lob.HostName,
                Picture = lob.Picture,
                Moods = lob.Moods,
                 CurrentPlayers = lob.Members.Count(m => m.Status != "Pending"),
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
                 CurrentPlayers = lob.Members.Count(m => m.Status != "Pending"),
                MaxPlayers = lob.MaxPlayers,
                Status = lob.GetStatus(),
                isRecuiting = lob.IsRecruiting

            }).ToList();

            // Completed lobbies ที่ user เป็นสมาชิก (รวม host ด้วย)
            var completedLobbies = new List<ShowLobbyDTO>();
            if (!string.IsNullOrEmpty(userId))
            {
                var completedFilter = Builders<Lobby>.Filter.And(
                    Builders<Lobby>.Filter.Eq(l => l.IsComplete, true),
                    Builders<Lobby>.Filter.ElemMatch(l => l.Members,
                        m => m.UserId == userId && m.Status != "Pending")
                );
                var completedList = await _database.Lobbies
                    .Find(completedFilter)
                    .SortByDescending(l => l.EndEvent)
                    .Limit(20)
                    .ToListAsync();

                var completedHostIds = completedList.Select(l => l.HostId).Where(id => id != null).Distinct().ToList();
                var completedHostUsers = completedHostIds.Any()
                    ? await _database.Users.Find(u => completedHostIds.Contains(u.Id)).ToListAsync()
                    : new List<User>();
                var completedHostMap = completedHostUsers.ToDictionary(u => u.Id);

                completedLobbies = completedList.Select(lob => new ShowLobbyDTO
                {
                    Id = lob.Id,
                    Title = lob.Title,
                    Game = lob.Game,
                    Description = lob.Description,
                    HostName = completedHostMap.TryGetValue(lob.HostId ?? "", out var ch) ? ch.Username : lob.HostName,
                    Picture = lob.Picture,
                    Moods = lob.Moods,
                    CurrentPlayers = lob.Members.Count(m => m.Status != "Pending"),
                    MaxPlayers = lob.MaxPlayers,
                    Status = lob.GetStatus(),
                    isRecuiting = false
                }).ToList();
            }

            return new LobbyListResponse
            {
                MyLobbies = myLobby,
                OtherLobbies = publicLobby,
                CompletedLobbies = completedLobbies
            };
        
            }
            
            
        public async Task<List<ShowLobbyDTO>> GetNextLobbiesAsync(string? lastId,string? userId, int pageSize = 10)
            {

                var notYours = Builders<Lobby>.Filter.Ne(l => l.HostId, userId);
                var now = DateTime.UtcNow;
                var isRecruiting = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Gte(l => l.EndRecruiting, now),
                Builders<Lobby>.Filter.Lte(l => l.StartRecruiting, now),
                Builders<Lobby>.Filter.Eq(l => l.IsRecruiting, true),
                Builders<Lobby>.Filter.Eq(l => l.IsComplete, false)
                 );
                

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
                     CurrentPlayers = lob.Members.Count(m => m.Status != "Pending"),
                    MaxPlayers = lob.MaxPlayers,
                    Status = lob.GetStatus(),
                    isRecuiting = lob.IsRecruiting
                    }).ToList();
                }

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
                

                Console.WriteLine($"Inserting Lobby: {System.Text.Json.JsonSerializer.Serialize(lobby)}");

                await _database.Lobbies.InsertOneAsync(lobby);
                

                return (true, "OK");
            }
            
            catch (MongoException ex) {

                Console.WriteLine($"MongoDB Error: {ex.Message}");
                return (false, ex.Message);
            }
            catch (Exception ex) {

                Console.WriteLine($"General Error: {ex.Message}");
                return (false, ex.Message);
            }
        }

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

            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return (false, "Lobby not found.");

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

            var roleDef = lobby.Roles.FirstOrDefault(r => r.Name == role);
            if (roleDef != null)
            {

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
        {

            var lobby = await _database.Lobbies.Find(l => l.Id == lobbyId).FirstOrDefaultAsync();
            if (lobby == null) return false;

            var filter = Builders<Lobby>.Filter.And(
                Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId),
                Builders<Lobby>.Filter.ElemMatch(l => l.Members, m => m.UserId == userId && m.Status == "Pending")
            );
            var update = Builders<Lobby>.Update.Set("Members.$.Status", "joined");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);

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

        public async Task<bool> SubmitKarmaAsync(string lobbyId, string fromUserId, string targetUserId, double score, string comment = "")
        {
            // เช็คว่าเคย rate targetUser ใน lobby นี้แล้วหรือยัง
            var alreadyRated = await _database.KarmaHistories
                .Find(k => k.LobbyId == lobbyId &&
                           k.FromUserId == fromUserId &&
                           k.TargetUserId == targetUserId)
                .AnyAsync();

            if (alreadyRated) return false; // ห้าม rate ซ้ำใน lobby เดิม

            // ถ้าไม่มี comment ให้ใช้ label ตาม score
            if (string.IsNullOrWhiteSpace(comment))
            {
                comment = score switch
                {
                     5  => "MVP",
                     4  => "Leader",
                     3  => "Good",
                     2  => "Chill",
                    -3  => "AFK",
                    -5  => "Toxic",
                    _   => score >= 0 ? "Good" : "Bad"
                };
            }

            var karmaHistory = new KarmaHistory
            {
                LobbyId      = lobbyId,
                FromUserId   = fromUserId,
                TargetUserId = targetUserId,
                Score        = score,
                Comment      = comment,
                Date         = DateTime.UtcNow
            };

            await _database.KarmaHistories.InsertOneAsync(karmaHistory);

            // Recalculate KarmaScore จาก KarmaHistory ทั้งหมด (ทุก lobby)
            var allHistories = await _database.KarmaHistories
                .Find(k => k.TargetUserId == targetUserId)
                .ToListAsync();

            if (allHistories.Any())
            {
                var newAvg     = Math.Round(allHistories.Average(k => k.Score), 2);
                var userFilter = Builders<User>.Filter.Eq(u => u.Id, targetUserId);
                var userUpdate = Builders<User>.Update.Set(u => u.KarmaScore, newAvg);
                await _database.Users.UpdateOneAsync(userFilter, userUpdate);
            }

            var sender     = await _database.Users.Find(u => u.Id == fromUserId).FirstOrDefaultAsync();
            var senderName = sender?.Username ?? "เพื่อนร่วมทีม";
            var notification = new Notification
            {
                Type           = "karma_received",
                RelateObjectId = fromUserId,
                UserId         = targetUserId,
                Text           = $"{senderName} ให้ Karma คุณ: {comment} ({(score > 0 ? "+" : "")}{score})",
                IsRead         = false,
                Date           = DateTime.Now
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

            if (newRole != "Other")
            {
                var roleDef = lobby.Roles.FirstOrDefault(r => r.Name == newRole);
                if (roleDef == null) return false;

                var takenCount = lobby.Members.Count(m =>
                    m.Role == newRole && m.UserId != userId && m.Status != "Pending");

                if (takenCount >= roleDef.Quantity)
                    return false;
            }

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

            var lobby = await _database.Lobbies
                .Find(l => l.Id == id)
                .FirstOrDefaultAsync();

            if (lobby == null) return null;

            var memberIds = lobby.Members
                .Where(m => m.Status != "Pending" && m.Status != "Invited")
                .Select(m => m.UserId)
                .ToList();
            
            var memberUsers = memberIds.Any()
                ? await _database.Users.Find(u => memberIds.Contains(u.Id)).ToListAsync()
                : new List<User>();

            var memberMap = memberUsers.ToDictionary(u => u.Id);

            bool isHost   = currentUserId == lobby.HostId;
            bool isMember = lobby.Members.Any(m =>
                m.UserId == currentUserId && m.Status != "Pending" && m.Status != "Invited");

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

            var endorsedUserIds = new HashSet<string>();
            if (lobby.IsComplete && !string.IsNullOrEmpty(currentUserId))
            {
                // filter ด้วย LobbyId ด้วย → rate ซ้ำข้าม lobby ได้ แต่ใน lobby เดิมห้าม
                var karmaGiven = await _database.KarmaHistories
                    .Find(k => k.LobbyId  == id &&
                               k.FromUserId == currentUserId &&
                               memberIds.Contains(k.TargetUserId))
                    .ToListAsync();
                endorsedUserIds = karmaGiven.Select(k => k.TargetUserId).ToHashSet();
            }

            User? currentUser = null;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                currentUser = memberMap.TryGetValue(currentUserId, out var cu)
                    ? cu
                    : await _database.Users.Find(u => u.Id == currentUserId).FirstOrDefaultAsync();
            }

            var invitableFriends = new List<User>();
            bool hasPendingInvite = false;
            string? invitedByName = null;

            if (!string.IsNullOrEmpty(currentUserId))
            {

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

                var markFilter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
                var markUpdate = Builders<Lobby>.Update
                    .Set(l => l.AutoRecruitProcessed, true)
                    .Set(l => l.IsRecruiting, false);
                await _database.Lobbies.UpdateOneAsync(markFilter, markUpdate);
                return;
            }

            var pendingUserIds = pendingMembers.Select(m => m.UserId).ToList();
            var pendingUsers = await _database.Users.Find(u => pendingUserIds.Contains(u.Id)).ToListAsync();
            var userKarmaMap = pendingUsers.ToDictionary(u => u.Id, u => u.KarmaScore);

            var sorted = pendingMembers
                .OrderByDescending(m => userKarmaMap.GetValueOrDefault(m.UserId, 0))
                .ThenBy(m => m.AppliedAt)
                .ToList();

            var currentJoined = lobby.Members.Count(m => m.Status != "Pending" && m.Status != "Invited");
            var availableSlots = Math.Max(0, lobby.MaxPlayers - currentJoined);

            var accepted = sorted.Take(availableSlots).ToList();
            var rejected = sorted.Skip(availableSlots).ToList();

            foreach (var member in accepted)
            {
                member.Status = "joined";
                member.Role = "Other";
            }

            foreach (var member in rejected)
            {
                lobby.Members.Remove(member);
            }

            lobby.AutoRecruitProcessed = true;
            lobby.IsRecruiting = false;

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            await _database.Lobbies.ReplaceOneAsync(filter, lobby);

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

            var inviterMember = lobby.Members.FirstOrDefault(m => m.UserId == inviterId && (m.Status == "joined" || m.Status == "Host"));
            if (inviterMember == null)
                return (false, "You must be a member to invite friends.");

            if (lobby.Members.Any(m => m.UserId == friendId))
                return (false, "This user is already in the lobby.");

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

            var filter = Builders<Lobby>.Filter.Eq(l => l.Id, lobbyId);
            var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId && m.Status == "Invited");
            var result = await _database.Lobbies.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0 && member.InvitedBy != null)
            {
                var friendUser = await _database.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                var friendName = friendUser?.Username ?? "Someone";

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
                var yours = builder.Eq(l => l.HostId, userId);
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
                    nextLobby = await _database.Lobbies.Find(filter & ((notYours & isRecruiting) | yours))
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
                     CurrentPlayers = lob.Members.Count(m => m.Status != "Pending"),
                    MaxPlayers = lob.MaxPlayers,
                    isRecuiting = lob.IsRecruiting,
                    Status = lob.GetStatus(),
                    HostId = lob.HostId

                    }).ToList();
        }
    }

    
       

}