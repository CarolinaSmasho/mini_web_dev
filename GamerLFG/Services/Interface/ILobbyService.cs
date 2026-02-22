using GamerLFG.Models;
using GamerLFG.Services.Interface.DTOs;
namespace GamerLFG.Services.Interface
{
    public interface ILobbyService
    {
        Task<List<ShowLobbyDTO>> GetAllLobbyAsync();
        Task CreateLobbyAsync(CreateLobbyDTO newLobby);
        Task DeleteLobbyAsync (string id);
        Task UpdateLobbyAsync (Lobby lobby);
        Task AddmemberAsync (Lobby current_lobby,User newUser);
    }
}