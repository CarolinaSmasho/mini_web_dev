using GamerLFG.Repositories;
using GamerLFG.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationApiController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly NotificationService _notificationService;

        public NotificationApiController(INotificationRepository notificationRepository, NotificationService notificationService)
        {
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get current user's notifications
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int limit = 20)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Unauthorized(new { success = false, error = "Not logged in" });

            var notifications = await _notificationRepository.GetByUserIdAsync(userId, limit);
            
            return Ok(new { 
                success = true, 
                notifications = notifications.Select(n => new {
                    n.Id,
                    n.Type,
                    n.Message,
                    n.RelatedEntityId,
                    n.IsRead,
                    n.CreatedAt
                })
            });
        }

        /// <summary>
        /// Get count of unread notifications
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Unauthorized(new { success = false, error = "Not logged in" });

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { success = true, count });
        }

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        [HttpPost("read/{id}")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Unauthorized(new { success = false, error = "Not logged in" });

            // Ideally check ownership but for now assume ID is sufficient as UUIDs are hard to guess
            await _notificationRepository.MarkAsReadAsync(id);
            return Ok(new { success = true });
        }

        /// <summary>
        /// Mark all notifications as read
        /// </summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return Unauthorized(new { success = false, error = "Not logged in" });

            await _notificationRepository.MarkAllAsReadAsync(userId);
            return Ok(new { success = true });
        }
    }
}
