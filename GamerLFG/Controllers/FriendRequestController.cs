using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace GamerLFG.Controllers;
[Authorize]
public class FriendRequestController : Controller
{
    public class FriendRequestViewModel
    {
        public string RequestId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public string SentAt { get; set; }
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
        string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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

    [HttpPost]
    public async Task<IActionResult> Cancel(string targetUserId)
    {
        string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool success = await _friendRequestService.CancelRequestAsync(currentUserId, targetUserId);
        
        if (success) return Json(new { success = true, message = "ยกเลิกคำขอแล้ว" });
        return BadRequest(new { success = false, message = "ไม่สามารถยกเลิกได้" });
    }
    
    [HttpGet]
    public async Task<IActionResult> FriendRequestList()
    {

      
        string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var requestList = await _friendRequestService.GetPendingRequestsAsync(currentUserId);

        var requestViewModels = new List<FriendRequestViewModel>();

        foreach (var req in requestList)
        {
            var sender = await _userService.SearchUserAsync(req.UserSender);

            if (sender != null)
            {

                requestViewModels.Add(new FriendRequestViewModel
                {
                    RequestId = req.Id,
                    SenderId = sender.Id,
                    SenderName = sender.Name,
                    SenderAvatar = sender.Avatar,

                    SentAt = req.CreatedAt.ToLocalTime().ToString("d/M/yyyy", new System.Globalization.CultureInfo("th-TH"))
                });
            }
        }

        return View(requestViewModels);
    }

}

