using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public interface INotificationRepository
    {
        Task CreateAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(string userId, int limit = 20);
        Task<long> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(string id);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteAsync(string id);
    }
}
