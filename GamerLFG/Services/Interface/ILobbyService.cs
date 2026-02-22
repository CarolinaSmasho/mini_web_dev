using GamerLFG.Models;

namespace GamerLFG.Services.Interface
{
    public interface ILobbyService
    {
        Task<List<Lobby>> GetAllLobbyAsync();
        Task<Lobby> CreateLobbyAsync(Lobby newLobby);
        Task DeleteLobbyAsync (string id);
        Task UpdateLobbyAsync (Lobby lobby);
        Task AddmemberAsync (Lobby current_lobby,User newUser);
    }
}