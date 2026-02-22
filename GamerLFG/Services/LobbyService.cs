using GamerLFG.Models;
using GamerLFG.Repositories;

namespace GamerLFG.Services
{
    public class LobbyService : ILobbyService
    {
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IEndorsementRepository _endorsementRepository;
        private readonly KarmaService _karmaService;
        private readonly NotificationService _notificationService;

        public LobbyService(
            ILobbyRepository lobbyRepository,
            IUserRepository userRepository,
            IApplicationRepository applicationRepository,
            IEndorsementRepository endorsementRepository,
            KarmaService karmaService,
            NotificationService notificationService)
        {
            _lobbyRepository = lobbyRepository;
            _userRepository = userRepository;
            _applicationRepository = applicationRepository;
            _endorsementRepository = endorsementRepository;
            _karmaService = karmaService;
            _notificationService = notificationService;
        }

        public async Task<List<Lobby>> GetLobbiesAsync(string? game = null)
        {
            var lobby = new Lobby();
            var lobbies = await _lobbyRepository.GetLobbiesAsync();
            if (!string.IsNullOrEmpty(game))
                lobbies = lobbies.Where(l => l.Game.Contains(game, StringComparison.OrdinalIgnoreCase)).ToList();
            return lobbies;
        }

        public async Task<Lobby?> GetLobbyAsync(string id)
        {
            return await _lobbyRepository.GetLobbyAsync(id);
        }

        public async Task<string> CreateLobbyAsync(string userId, Lobby lobby, List<string> moods, List<string> roleNames, List<int> roleCounts, string? hostRole)
        {
            lobby.Roles = BuildRoles(roleNames, roleCounts, lobby.MaxPlayers);
            lobby.Moods = moods ?? new List<string>();
            lobby.HostId = userId;

            var assignedRole = !string.IsNullOrEmpty(hostRole)
                ? hostRole
                : (lobby.Roles.FirstOrDefault()?.Name ?? "Host");

            lobby.Members = new List<Member>
            {
                new Member
                {
                    UserId = userId,
                    AssignedRole = assignedRole,
                    IsHost = true,
                    JoinedAt = DateTime.UtcNow
                }
            };

            var hostRoleObj = lobby.Roles.FirstOrDefault(r => r.Name == assignedRole);
            if (hostRoleObj != null) hostRoleObj.Filled = 1;

            lobby.RecruitmentDeadline = ToUtc(lobby.RecruitmentDeadline);
            lobby.SessionStartTime = ToUtc(lobby.SessionStartTime);
            lobby.SessionEndTime = ToUtc(lobby.SessionEndTime);
            lobby.CreatedAt = DateTime.UtcNow;

            await _lobbyRepository.CreateLobbyAsync(lobby);
            return lobby.Id;
        }

        public async Task UpdateLobbyAsync(string id, string userId, Lobby updatedLobby, List<string> moods, List<string> roleNames, List<int> roleCounts)
        {
            var existingLobby = await _lobbyRepository.GetLobbyAsync(id);
            if (existingLobby == null) return;

            existingLobby.Roles = BuildRoles(roleNames, roleCounts, updatedLobby.MaxPlayers);

            // Recalculate Filled based on current members
            foreach (var role in existingLobby.Roles)
                role.Filled = existingLobby.Members.Count(m => m.AssignedRole == role.Name);

            existingLobby.Title = updatedLobby.Title;
            existingLobby.Game = updatedLobby.Game;
            existingLobby.Description = updatedLobby.Description;
            existingLobby.MaxPlayers = updatedLobby.MaxPlayers;
            existingLobby.PictureUrl = updatedLobby.PictureUrl;
            existingLobby.DiscordLink = updatedLobby.DiscordLink;
            existingLobby.Moods = moods ?? new List<string>();

            existingLobby.RecruitmentDeadline = ToUtc(updatedLobby.RecruitmentDeadline) ?? existingLobby.RecruitmentDeadline;
            existingLobby.SessionStartTime = ToUtc(updatedLobby.SessionStartTime) ?? existingLobby.SessionStartTime;
            existingLobby.SessionEndTime = ToUtc(updatedLobby.SessionEndTime) ?? existingLobby.SessionEndTime;

            await _lobbyRepository.UpdateLobbyAsync(existingLobby);
        }

        public async Task DeleteLobbyAsync(string id)
        {
            await _lobbyRepository.DeleteLobbyAsync(id);
            await _applicationRepository.DeleteApplicationsByLobbyIdAsync(id);
        }

        public async Task<List<Application>> GetApplicationsByLobbyIdAsync(string lobbyId)
        {
            return await _applicationRepository.GetApplicationsByLobbyIdAsync(lobbyId);
        }

        public async Task<List<Application>> GetApplicationsByUserIdAsync(string userId)
        {
            return await _applicationRepository.GetApplicationsByUserIdAsync(userId);
        }

        public async Task<(bool success, string? error)> ApplyAsync(string lobbyId, string userId, string? role)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(lobbyId);
            if (lobby == null) return (false, "Lobby not found");

            if (lobby.RecruitmentDeadline.HasValue && DateTime.UtcNow > lobby.RecruitmentDeadline.Value)
                return (false, "Recruitment period has ended for this lobby.");

            if (lobby.Members.Any(m => m.UserId == userId))
                return (false, "You are already in this lobby");

            var existingApps = await _applicationRepository.GetApplicationsByLobbyIdAsync(lobbyId);
            if (existingApps.Any(a => a.UserId == userId && a.Status == "Pending"))
                return (false, "Application already pending");

            var application = new Application
            {
                LobbyId = lobbyId,
                UserId = userId,
                DesiredRoles = !string.IsNullOrEmpty(role) ? new List<string> { role } : new List<string>(),
                Message = string.IsNullOrEmpty(role) ? "Requesting to join" : $"Applying for {role}",
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };

            await _applicationRepository.CreateApplicationAsync(application);

            var user = await _userRepository.GetUserAsync(userId);
            await _notificationService.NotifyUserAsync(lobby.HostId, "Application",
                $"New application from {user?.Username ?? "Unknown"} for {lobby.Title}", application.Id);

            return (true, null);
        }

        public async Task CancelApplicationAsync(string lobbyId, string userId)
        {
            await _lobbyRepository.RemoveMemberAsync(lobbyId, userId);

            var applications = await _applicationRepository.GetApplicationsByUserIdAsync(userId);
            var lobbyApp = applications.FirstOrDefault(a => a.LobbyId == lobbyId);
            if (lobbyApp != null)
                await _applicationRepository.DeleteApplicationAsync(lobbyApp.Id);
        }

        public async Task<(bool success, string? error)> RecruitAsync(string applicationId, string hostId)
        {
            var application = await _applicationRepository.GetApplicationAsync(applicationId);
            if (application == null) return (false, "Application not found");

            var lobby = await _lobbyRepository.GetLobbyAsync(application.LobbyId);
            if (lobby == null) return (false, "Lobby not found");

            if (lobby.HostId != hostId) return (false, "Not authorized");
            if (lobby.Members.Count >= lobby.MaxPlayers) return (false, "Lobby is full");
            if (lobby.Members.Any(m => m.UserId == application.UserId)) return (false, "User is already a member");

            await _applicationRepository.UpdateApplicationStatusAsync(applicationId, "Accepted");

            var role = application.DesiredRoles?.FirstOrDefault() ?? "Member";
            var member = new Member
            {
                UserId = application.UserId,
                AssignedRole = role,
                IsHost = false,
                JoinedAt = DateTime.UtcNow
            };

            await _lobbyRepository.AddMemberAsync(lobby.Id, member);
            await _karmaService.AwardLobbyAcceptedAsync(application.UserId, lobby.Id, lobby.Title);
            await _notificationService.NotifyUserAsync(application.UserId, "Recruitment",
                $"You have been recruited to {lobby.Title}!", lobby.Id);

            return (true, null);
        }

        public async Task<(bool success, string? error)> RejectAsync(string applicationId, string hostId)
        {
            var application = await _applicationRepository.GetApplicationAsync(applicationId);
            if (application == null) return (false, "Application not found");

            var lobby = await _lobbyRepository.GetLobbyAsync(application.LobbyId);
            if (lobby == null) return (false, "Lobby not found");

            if (lobby.HostId != hostId) return (false, "Not authorized");

            await _applicationRepository.UpdateApplicationStatusAsync(applicationId, "Rejected");
            await _notificationService.NotifyUserAsync(application.UserId, "Recruitment",
                $"Your application to {lobby.Title} was declined.", lobby.Id);

            return (true, null);
        }

        public async Task<(bool success, string? error)> KickAsync(string lobbyId, string memberId, string hostId, bool hardKick)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(lobbyId);
            if (lobby == null) return (false, "Lobby not found");
            if (lobby.HostId != hostId) return (false, "Not authorized");
            if (memberId == lobby.HostId) return (false, "Cannot kick host");

            await _lobbyRepository.RemoveMemberAsync(lobbyId, memberId);
            await _karmaService.PenalizeKickedAsync(memberId, lobbyId, lobby.Title, hardKick);
            await _notificationService.NotifyUserAsync(memberId, "System",
                $"You were removed from lobby {lobby.Title}.", lobbyId);

            return (true, null);
        }

        public async Task<(bool success, string? error)> UpdateBackgroundAsync(string id, string pictureUrl, string userId)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return (false, "Lobby not found");
            if (lobby.HostId != userId) return (false, "Not authorized");

            lobby.PictureUrl = pictureUrl;
            await _lobbyRepository.UpdateLobbyAsync(lobby);
            return (true, null);
        }

        public async Task<(bool success, bool isRecruiting, string? error)> ToggleRecruitmentAsync(string id, string userId)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || lobby.HostId != userId) return (false, false, "Unauthorized");

            lobby.IsRecruiting = !lobby.IsRecruiting;
            await _lobbyRepository.UpdateLobbyAsync(lobby);
            return (true, lobby.IsRecruiting, null);
        }

        public async Task<(bool success, string? error)> CompleteSessionAsync(string id, string userId)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || lobby.HostId != userId) return (false, "Unauthorized");

            lobby.IsCompleted = true;
            lobby.Status = "Closed";
            await _lobbyRepository.UpdateLobbyAsync(lobby);

            await _karmaService.AwardLobbyCompletedHostAsync(lobby.HostId, lobby.Id, lobby.Title);
            foreach (var member in lobby.Members.Where(m => !m.IsHost))
                await _karmaService.AwardLobbyCompletedMemberAsync(member.UserId, lobby.Id, lobby.Title);

            return (true, null);
        }

        public async Task<(bool success, string? error)> EndorseMemberAsync(string lobbyId, string fromUserId, string targetUserId, string type)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(lobbyId);
            if (lobby == null || !lobby.IsCompleted) return (false, "Session not completed");

            if (!lobby.Members.Any(m => m.UserId == fromUserId) || !lobby.Members.Any(m => m.UserId == targetUserId))
                return (false, "Invalid members");

            if (await _endorsementRepository.HasEndorsedInLobbyAsync(fromUserId, targetUserId, lobbyId))
                return (false, "You have already evaluated this member for this mission.");

            var endorsement = new Endorsement
            {
                FromUserId = fromUserId,
                ToUserId = targetUserId,
                EndorsementType = type,
                Comment = "Session Feedback",
                LobbyId = lobbyId,
                CreatedAt = DateTime.UtcNow
            };

            await _endorsementRepository.CreateEndorsementAsync(endorsement);

            var fromUser = await _userRepository.GetUserAsync(fromUserId);
            await _karmaService.ProcessEndorsementAsync(targetUserId, fromUser?.Username ?? "Unknown", type, endorsement.Id);

            return (true, null);
        }

        private static List<Role> BuildRoles(List<string> roleNames, List<int> roleCounts, int maxPlayers)
        {
            var roles = new List<Role>();
            int total = 0;

            if (roleNames != null && roleCounts != null)
            {
                for (int i = 0; i < roleNames.Count; i++)
                {
                    if (i < roleCounts.Count && !string.IsNullOrWhiteSpace(roleNames[i]))
                    {
                        int count = roleCounts[i];
                        if (total + count > maxPlayers) count = maxPlayers - total;
                        if (count > 0)
                        {
                            roles.Add(new Role { Name = roleNames[i], Count = count, Filled = 0 });
                            total += count;
                        }
                    }
                    if (total >= maxPlayers) break;
                }
            }

            if (total < maxPlayers)
                roles.Add(new Role { Name = "Other", Count = maxPlayers - total, Filled = 0 });

            return roles;
        }

        private static DateTime? ToUtc(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            return DateTime.SpecifyKind(dt.Value, DateTimeKind.Unspecified).ToUniversalTime();
        }
    }
}
