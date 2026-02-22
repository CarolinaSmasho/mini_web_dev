using GamerLFG.Models;
using GamerLFG.Services.Interface.DTOs;
namespace GamerLFG.Services.Interface
{
    public interface ILobbyService
    {
        Task<LobbyListResponse> GetAllLobbyAsync(string? userId);
        Task<List<ShowLobbyDTO>> GetNextLobbiesAsync(string? lastId, int pageSize = 10,string? userId);
        Task CreateLobbyAsync(CreateLobbyDTO newLobby);
        Task DeleteLobbyAsync (string id);
        Task UpdateLobbyAsync (Lobby lobby);
        Task AddmemberAsync (Lobby current_lobby,User newUser);
        
    }
}