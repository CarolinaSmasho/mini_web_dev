using GamerLFG.Models;
using GamerLFG.Models.ViewModels;
using GamerLFG.Services.Interface.DTOs;
namespace GamerLFG.Services.Interface
{
    public interface ILobbyService
    {
        Task<LobbyListResponse> GetAllLobbyAsync(string? userId);
        Task<List<ShowLobbyDTO>> GetNextLobbiesAsync(string? lastId,string? userId, int pageSize = 10);

        Task <(bool success,string message)> CreateLobbyAsync(CreateLobbyDTO newLobby);
        Task<bool> DeleteLobbyAsync(string id);
        Task UpdateLobbyAsync (Lobby lobby);
        Task AddmemberAsync (Lobby current_lobby,User newUser);

        Task<LobbyDetailsViewModel?> GetLobbyDetailsAsync(string id, string? currentUserId);

        Task<Lobby?> GetLobbyByIdAsync(string id);
        Task<(bool success, string message)> ApplyToLobbyAsync(string lobbyId, string userId, string role);
        Task<bool> CancelApplicationAsync(string lobbyId, string userId);
        Task<bool> RecruitMemberAsync(string lobbyId, string userId);
        Task<bool> RejectApplicantAsync(string lobbyId, string userId);
        Task<bool> KickMemberAsync(string lobbyId, string userId);
        Task<bool> CompleteLobbyAsync(string lobbyId);
        Task<bool> SubmitKarmaAsync(string lobbyId, string fromUserId, string targetUserId, double score, string comment = "");
        Task<bool> ChangeMemberRoleAsync(string lobbyId, string userId, string newRole);
        Task<bool> ToggleRecruitmentAsync(string lobbyId);
        Task<List<ShowLobbyDTO>> GetLobbiesAsyncByName(string? lobName,string? userId,string userName = "", int pageSize = 10);
        Task ProcessAutoRecruitAsync(string lobbyId);
        Task<(bool success, string message)> InviteFriendAsync(string lobbyId, string inviterId, string friendId, string role);
        Task<(bool success, string message)> AcceptInviteAsync(string lobbyId, string userId);
        Task<(bool success, string message)> DeclineInviteAsync(string lobbyId, string userId);
    }
}