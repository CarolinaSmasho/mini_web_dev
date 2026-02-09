using GamerLFG.Models;
using GamerLFG.Repositories;
using System;
using System.Threading.Tasks;

namespace GamerLFG.Services
{
    public class NotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task NotifyUserAsync(string userId, string type, string message, string? relatedEntityId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task<long> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }
    }
}
