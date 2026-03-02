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
        Task DeleteLobbyAsync (string id);
        Task UpdateLobbyAsync (Lobby lobby);
        Task AddmemberAsync (Lobby current_lobby,User newUser);

        /// <summary>ดึงข้อมูลทั้งหมดที่ Lobby/Details ต้องการ รวมถึง members, applications และ karma</summary>
        Task<LobbyDetailsViewModel?> GetLobbyDetailsAsync(string id, string? currentUserId);
    }
}