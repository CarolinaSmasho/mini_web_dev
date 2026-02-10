using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface IApplicationRepository
    {
        Task<Application> GetApplicationAsync(string id);
        Task<List<Application>> GetApplicationsByLobbyIdAsync(string lobbyId);
        Task<List<Application>> GetApplicationsByUserIdAsync(string userId);
        Task CreateApplicationAsync(Application application);
        Task UpdateApplicationStatusAsync(string id, string status);
        Task DeleteApplicationAsync(string id); // Cancel
        Task DeleteApplicationsByLobbyIdAsync(string lobbyId);
    }
}
