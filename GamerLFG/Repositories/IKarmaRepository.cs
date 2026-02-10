using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface IKarmaRepository
    {
        Task CreateAsync(KarmaHistory entry);
        Task<List<KarmaHistory>> GetByUserIdAsync(string userId, int limit = 50);
        Task<int> GetTotalPointsByUserIdAsync(string userId);
    }
}
