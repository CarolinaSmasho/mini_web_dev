using GamerLFG.Models;
using GamerLFG.Repositories;

namespace GamerLFG.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEndorsementRepository _endorsementRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly KarmaService _karmaService;
        private readonly NotificationService _notificationService;

        public UserService(
            IUserRepository userRepository,
            IEndorsementRepository endorsementRepository,
            IFriendRequestRepository friendRequestRepository,
            KarmaService karmaService,
            NotificationService notificationService)
        {
            _userRepository = userRepository;
            _endorsementRepository = endorsementRepository;
            _friendRequestRepository = friendRequestRepository;
            _karmaService = karmaService;
            _notificationService = notificationService;
        }

        public async Task<User?> GetUserAsync(string id) =>
            await _userRepository.GetUserAsync(id);

        public async Task<List<User>> GetUsersAsync(List<string> ids) =>
            await _userRepository.GetUsersAsync(ids);

        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _userRepository.GetUserByUsernameAsync(username);

        public async Task<List<User>> SearchUsersAsync(string query) =>
            await _userRepository.SearchUsersAsync(query);

        public async Task<List<Endorsement>> GetEndorsementsFromUserInLobbyAsync(string fromUserId, string lobbyId) =>
            await _endorsementRepository.GetEndorsementsFromUserInLobbyAsync(fromUserId, lobbyId);

        public async Task UpdateProfileAsync(string userId, string? username, string? bio, string? gamesPlayedInput, string? discordUserId, string? steamId, string? twitchChannel)
        {
            var user = await _userRepository.GetUserAsync(userId);
            if (user == null) return;

            if (!string.IsNullOrEmpty(username)) user.Username = username;
            user.Bio = bio ?? user.Bio;

            if (!string.IsNullOrEmpty(gamesPlayedInput))
            {
                user.GameLibrary = gamesPlayedInput
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => g.Trim())
                    .ToList();
            }

            user.DiscordUserId = discordUserId ?? user.DiscordUserId;
            user.SteamId = steamId ?? user.SteamId;
            user.TwitchChannel = twitchChannel ?? user.TwitchChannel;

            await _userRepository.UpdateUserAsync(user);
        }

        public async Task<List<User>> GetFriendsAsync(string userId)
        {
            var user = await _userRepository.GetUserAsync(userId);
            if (user?.FriendIds == null || !user.FriendIds.Any()) return new List<User>();
            return await _userRepository.GetUsersAsync(user.FriendIds);
        }

        public async Task AddFriendAsync(string userId, string friendId)
        {
            await _userRepository.AddFriendAsync(userId, friendId);
        }

        public async Task<(bool success, string? error)> SendFriendRequestAsync(string fromUserId, string toUserId)
        {
            var fromUser = await _userRepository.GetUserAsync(fromUserId);
            var toUser = await _userRepository.GetUserAsync(toUserId);

            if (fromUser == null || toUser == null) return (false, "User not found");
            if (fromUser.FriendIds.Contains(toUserId)) return (false, "Already friends");

            var existing = await _friendRequestRepository.GetByUsersAsync(fromUserId, toUserId);
            if (existing != null && existing.Status == "Pending") return (false, "Request already pending");

            var newRequest = new FriendRequest
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _friendRequestRepository.CreateAsync(newRequest);
            await _notificationService.NotifyUserAsync(toUserId, "FriendRequest",
                $"You have a friend request from {fromUser.Username}", newRequest.Id);

            return (true, null);
        }

        public async Task<(bool success, string? error)> AcceptFriendRequestAsync(string requestId, string userId)
        {
            var friendRequest = await _friendRequestRepository.GetByIdAsync(requestId);
            if (friendRequest == null) return (false, "Request not found");
            if (friendRequest.ToUserId != userId) return (false, "Not authorized");

            await _friendRequestRepository.UpdateStatusAsync(requestId, "Accepted");
            await _userRepository.AddFriendAsync(friendRequest.FromUserId, friendRequest.ToUserId);
            await _userRepository.AddFriendAsync(friendRequest.ToUserId, friendRequest.FromUserId);

            var user = await _userRepository.GetUserAsync(userId);
            await _notificationService.NotifyUserAsync(friendRequest.FromUserId, "FriendRequest",
                $"{user?.Username ?? "Unknown"} accepted your friend request", requestId);

            return (true, null);
        }

        public async Task<(bool success, string? error)> RejectFriendRequestAsync(string requestId, string userId)
        {
            var friendRequest = await _friendRequestRepository.GetByIdAsync(requestId);
            if (friendRequest == null) return (false, "Request not found");
            if (friendRequest.ToUserId != userId) return (false, "Not authorized");

            await _friendRequestRepository.UpdateStatusAsync(requestId, "Rejected");
            return (true, null);
        }

        public async Task<List<PendingRequestResult>> GetPendingRequestsAsync(string userId)
        {
            var requests = await _friendRequestRepository.GetPendingByUserIdAsync(userId);
            var fromUserIds = requests.Select(r => r.FromUserId).ToList();
            var fromUsers = await _userRepository.GetUsersAsync(fromUserIds);
            var userMap = fromUsers.ToDictionary(u => u.Id, u => u);

            return requests.Select(r => new PendingRequestResult(
                r.Id,
                r.FromUserId,
                r.CreatedAt,
                userMap.ContainsKey(r.FromUserId) ? userMap[r.FromUserId].Username : "Unknown",
                userMap.ContainsKey(r.FromUserId) ? userMap[r.FromUserId].AvatarUrl : ""
            )).ToList();
        }

        public async Task<List<Endorsement>> GetEndorsementsAsync(string userId) =>
            await _endorsementRepository.GetEndorsementsForUserAsync(userId);

        public async Task<(bool success, string? error, string? endorsementId)> GiveEndorsementAsync(
            string toUserId, string fromUserId, string type, string? comment)
        {
            var toUser = await _userRepository.GetUserAsync(toUserId);
            if (toUser == null) return (false, "User not found", null);

            var fromUser = await _userRepository.GetUserAsync(fromUserId);
            if (fromUser == null) return (false, "FromUser not found", null);

            var endorsement = new Endorsement
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                EndorsementType = type,
                Comment = comment ?? "",
                CreatedAt = DateTime.UtcNow
            };

            await _endorsementRepository.CreateEndorsementAsync(endorsement);
            await _karmaService.ProcessEndorsementAsync(toUserId, fromUser.Username, type, endorsement.Id);

            return (true, null, endorsement.Id);
        }
    }
}
