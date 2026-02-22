using GamerLFG.Models;
using GamerLFG.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Services
{
    public interface ILobbyService
    {
        Task<List<Lobby>> GetAllLobbiesAsync();
        Task<Lobby?> GetLobbyByIdAsync(string id);
        Task<Lobby> CreateLobbyAsync(CreateLobbyDTO dto);
        Task<bool> ToggleRecruitmentAsync(string id);
        Task<bool> ApplyToLobbyAsync(string lobbyId, string userId);
    }
}