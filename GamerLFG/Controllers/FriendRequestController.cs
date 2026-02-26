using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;

namespace GamerLFG.Controllers;

public class FriendRequestController : Controller
{
    public class FriendRequestViewModel
    {
        public string RequestId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public DateTime SentAt { get; set; }
    }
    private readonly IFriendRequestService _friendRequestService;
    private readonly IUserService _userService;
    public FriendRequestController(IFriendRequestService friendRequestService, IUserService userService)
    {
        _friendRequestService = friendRequestService;
        _userService = userService;

    }


    [HttpPost]
    public async Task<IActionResult> Send(string targetUserId)
    {
        string currentUserId = Request.Cookies["CurrentUserId"] ?? "65d8a0b1c2d3e4f5a6b7c8d1";
        Console.WriteLine($"[DEBUG] พยายามส่งคำขอจาก: {currentUserId} ไปหา: {targetUserId}");
        bool success = await _friendRequestService.SendRequestAsync(currentUserId, targetUserId);

        if (success) return Json(new { success = true, message = "ส่งคำขอเป็นเพื่อนแล้ว!" });
        return BadRequest(Json(new { success = false, message = "ไม่สามารถส่งคำขอได้ (อาจส่งไปแล้ว)" }));
    }

    [HttpPost]
    public async Task<IActionResult> Accept(string requestId)
    {
        bool success = await _friendRequestService.AcceptRequestAsync(requestId);

        if (success) return Json(new { success = true, message = "ยอมรับคำขอแล้ว! ตอนนี้คุณเป็นเพื่อนกันแล้ว" });
        return BadRequest(Json(new { success = false, message = "เกิดข้อผิดพลาดในการยอมรับคำขอ" }));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(string requestId)
    {
        bool success = await _friendRequestService.RejectRequestAsync(requestId);
        if (success) return Json(new { success = true, message = "ปฏิเสธคำขอและลบทิ้งแล้ว" });
        return BadRequest(new { success = false, message = "เกิดข้อผิดพลาดในการลบคำขอ" });
    }
    
    [HttpGet]
    public async Task<IActionResult> FriendRequestList()
    {
        string currentUserId = Request.Cookies["CurrentUserId"] ?? "65d8a0b1c2d3e4f5a6b7c8d1";

        var requestList = await _friendRequestService.GetPendingRequestsAsync(currentUserId);

        // 🟢 2. เปลี่ยนมาใช้ List ของคลาสที่เราเพิ่งสร้าง
        var requestViewModels = new List<FriendRequestViewModel>();

        foreach (var req in requestList)
        {
            var sender = await _userService.SearchUserAsync(req.UserSender);

            if (sender != null)
            {
                // 🟢 3. หยิบข้อมูลใส่กล่อง (ไม่ต้องมีชื่อตัวแปรย่อยๆ เล็กๆ แล้ว ใช้ตัวพิมพ์ใหญ่ตามคลาสได้เลย)
                requestViewModels.Add(new FriendRequestViewModel
                {
                    RequestId = req.Id,
                    SenderId = sender.Id,
                    SenderName = sender.Username,
                    SenderAvatar = sender.Avatar,
                    SentAt = req.CreatedAt
                });
            }
        }

        // 🟢 4. จุดไคลแม็กซ์! เปลี่ยนจาก Json เป็น View และโยนกล่องข้อมูลไปให้มัน
        return View(requestViewModels);
    }

}

