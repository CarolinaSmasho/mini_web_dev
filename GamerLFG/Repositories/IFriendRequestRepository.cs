using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface IFriendRequestRepository
    {
        Task CreateAsync(FriendRequest request);
        Task<FriendRequest> GetByIdAsync(string id);
        Task<List<FriendRequest>> GetPendingByUserIdAsync(string userId);
        Task<FriendRequest> GetByUsersAsync(string fromUserId, string toUserId);
        Task UpdateStatusAsync(string id, string status);
        Task DeleteAsync(string id);
    }
}
