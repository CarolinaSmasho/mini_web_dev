using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface ILobbyRepository
    {
        Task<List<Lobby>> GetLobbiesAsync();
        Task<Lobby> GetLobbyAsync(string id);
        Task CreateLobbyAsync(Lobby lobby);
        Task UpdateLobbyAsync(Lobby lobby);
        Task DeleteLobbyAsync(string id);
        Task AddMemberAsync(string lobbyId, Member member);
        Task RemoveMemberAsync(string lobbyId, string userId);
    }
}
