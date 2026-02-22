using GamerLFG.Models;

namespace GamerLFG.Services
{
    public record PendingRequestResult(string Id, string FromUserId, DateTime CreatedAt, string FromUserName, string FromUserAvatar);

    public interface IUserService
    {
        Task<User?> GetUserAsync(string id);
        Task<List<User>> GetUsersAsync(List<string> ids);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> SearchUsersAsync(string query);
        Task<List<Endorsement>> GetEndorsementsFromUserInLobbyAsync(string fromUserId, string lobbyId);
        Task UpdateProfileAsync(string userId, string? username, string? bio, string? gamesPlayedInput, string? discordUserId, string? steamId, string? twitchChannel);
        Task<List<User>> GetFriendsAsync(string userId);
        Task AddFriendAsync(string userId, string friendId);
        Task<(bool success, string? error)> SendFriendRequestAsync(string fromUserId, string toUserId);
        Task<(bool success, string? error)> AcceptFriendRequestAsync(string requestId, string userId);
        Task<(bool success, string? error)> RejectFriendRequestAsync(string requestId, string userId);
        Task<List<PendingRequestResult>> GetPendingRequestsAsync(string userId);
        Task<List<Endorsement>> GetEndorsementsAsync(string userId);
        Task<(bool success, string? error, string? endorsementId)> GiveEndorsementAsync(string toUserId, string fromUserId, string type, string? comment);
    }
}
