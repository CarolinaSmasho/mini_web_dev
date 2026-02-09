using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface IEndorsementRepository
    {
        Task<List<Endorsement>> GetEndorsementsForUserAsync(string userId);
        Task CreateEndorsementAsync(Endorsement endorsement);
    }
}
