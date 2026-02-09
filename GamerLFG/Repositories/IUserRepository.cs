using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task AddFriendAsync(string userId, string friendId);
        Task<List<User>> GetUsersAsync(List<string> ids);
    }
}
