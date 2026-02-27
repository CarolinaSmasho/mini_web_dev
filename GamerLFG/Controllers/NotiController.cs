using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using GamerLFG.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Controllers
{
    public class NotiController : Controller
    {
        private readonly IMongoCollection<Notification> _notifications;

        // รับ MongoClient มาจาก Program.cs (เหมือน UserService)
        public NotiController(IMongoClient client, IConfiguration configuration)
        {
            var databaseName = configuration["MongoDB:DatabaseName"]; // หรือใส่ชื่อ DB ตรงๆ เช่น "GamerHubDB"
            var database = client.GetDatabase(databaseName);
            _notifications = database.GetCollection<Notification>("Notifications");
        }

        // 1. หน้าแสดงรายการแจ้งเตือนทั้งหมด
        public async Task<IActionResult> Notification()
        {
            // ดึงข้อมูลทั้งหมด เรียงจากใหม่ไปเก่า
            var notifications = await _notifications.Find(_ => true)
                                           .SortByDescending(n => n.Date)
                                           .ToListAsync();

            return View(notifications);
        }

        // 2. ฟังก์ชันอัปเดตสถานะ (Mark as Read)
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
    }
}