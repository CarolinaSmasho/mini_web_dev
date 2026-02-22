using GamerLFG.Models;

namespace GamerLFG.Services
{
    public interface ILobbyService
    {
        Task<List<Lobby>> GetLobbiesAsync(string? game = null);
        Task<Lobby?> GetLobbyAsync(string id);
        Task<string> CreateLobbyAsync(string userId, Lobby lobby, List<string> moods, List<string> roleNames, List<int> roleCounts, string? hostRole);
        Task UpdateLobbyAsync(string id, string userId, Lobby updatedLobby, List<string> moods, List<string> roleNames, List<int> roleCounts);
        Task DeleteLobbyAsync(string id);
        Task<List<Application>> GetApplicationsByLobbyIdAsync(string lobbyId);
        Task<List<Application>> GetApplicationsByUserIdAsync(string userId);
        Task<(bool success, string? error)> ApplyAsync(string lobbyId, string userId, string? role);
        Task CancelApplicationAsync(string lobbyId, string userId);
        Task<(bool success, string? error)> RecruitAsync(string applicationId, string hostId);
        Task<(bool success, string? error)> RejectAsync(string applicationId, string hostId);
        Task<(bool success, string? error)> KickAsync(string lobbyId, string memberId, string hostId, bool hardKick);
        Task<(bool success, string? error)> UpdateBackgroundAsync(string id, string pictureUrl, string userId);
        Task<(bool success, bool isRecruiting, string? error)> ToggleRecruitmentAsync(string id, string userId);
        Task<(bool success, string? error)> CompleteSessionAsync(string id, string userId);
        Task<(bool success, string? error)> EndorseMemberAsync(string lobbyId, string fromUserId, string targetUserId, string type);
    }
}
