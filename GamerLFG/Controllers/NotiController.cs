using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics.Contracts; 
namespace GamerLFG.Controllers
{
    [Authorize]
    public class NotiController : Controller
    {
        private readonly IMongoCollection<Notification> _notifications;

        //get Mongoclent form Program.cs
        public NotiController(IMongoClient client, IConfiguration configuration)
        {
            var databaseName = configuration["MongoDB:DatabaseName"]; 
            var database = client.GetDatabase(databaseName);
            _notifications = database.GetCollection<Notification>("Notifications");
        }

        // all noti
        public async Task<IActionResult> Notification()
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _notifications.Find(n => n.UserId == currentUserId)
                                           .SortByDescending(n => n.Date)
                                           .ToListAsync();

            return View(notifications);
        }

        
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new { success = false });
            // สั่ง Update ข้อมูลใน MongoDB
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            
            await _notifications.UpdateOneAsync(filter, update);

            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId)) return Json(new { count = 0 });
            // count unread of this id
        var count = await _notifications.CountDocumentsAsync(n => n.UserId == currentUserId && n.IsRead == false);
    
        return Json(new { count = count });
        }
    }
}