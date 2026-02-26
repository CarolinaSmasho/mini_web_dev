using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Services
{
    public interface IUserService
    {
        Task<List<User>> GetMyFriendsAsync(string myUserId);

        Task<List<User>> SearchFriendAsync(string keyword);
    
        Task<User> SearchUserAsync(string UserId);

        Task<bool> RemoveFriendAsync(string currentUserId, string targetFriendId);

    }
}