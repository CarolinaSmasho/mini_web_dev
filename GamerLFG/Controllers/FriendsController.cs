using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;

namespace GamerLFG.Controllers;

public class FriendsController : Controller
{

    public IActionResult Friends_list()
    {
        return View();
    }


    public IActionResult Friends_request()
    {
        return View();
    }


// Mock Data: ข้อมูลจำลอง (ในของจริงจะมาจาก DB)
private readonly List<dynamic> _mockUsers = new List<dynamic>
{
    new { Id = "7417146387", Username = "xldn", DisplayName = "qkrxldn", Status = "Offline", Bio = "ชอบเล่นเกม FPS มากๆ" },
    new { Id = "2895051752", Username = "IloveMEOWMUK", DisplayName = "IloveAUGAIG", Status = "Online", Bio = "แมวคือพระเจ้า" },
    new { Id = "1172879974", Username = "Sleep", DisplayName = "SpiritaulSniping", Status = "Away", Bio = "ง่วงนอนตลอดเวลา" }
};

[HttpGet]
public IActionResult GetUserDetails(string id)
{
    // ค้นหาข้อมูลจาก Mock Data ตาม ID ที่ส่งมา
    var user = _mockUsers.FirstOrDefault(u => u.Id == id);

    if (user == null)
    {
        return NotFound(); // ถ้าไม่เจอ
    }

    return Json(user); // ส่งข้อมูลกลับไปเป็น JSON
}


}
