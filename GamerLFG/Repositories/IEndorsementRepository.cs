using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface IEndorsementRepository
    {
        Task<List<Endorsement>> GetEndorsementsForUserAsync(string userId);
        Task<List<Endorsement>> GetEndorsementsFromUserInLobbyAsync(string fromUserId, string lobbyId);
        Task<bool> HasEndorsedInLobbyAsync(string fromUserId, string toUserId, string lobbyId);
        Task CreateEndorsementAsync(Endorsement endorsement);
    }
}
