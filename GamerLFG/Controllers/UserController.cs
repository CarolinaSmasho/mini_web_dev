using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;

namespace GamerLFG.Controllers;

public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly IFriendRequestService _friendRequestService;

    public UserController(IUserService userService, IFriendRequestService friendRequestService)
    {
        _userService = userService;
        _friendRequestService = friendRequestService;
    }
        
    public async Task<IActionResult> Friends_list()
        {

            string currentUserId = Request.Cookies["CurrentUserId"] ?? "65d8a0b1c2d3e4f5a6b7c8d1"; 

            var friendsList = await _userService.GetMyFriendsAsync(currentUserId);


            return View(friendsList);
        }

    [HttpGet]
    public async Task<IActionResult> SearchAPI(string keyword)
    {
        var users = await _userService.SearchFriendAsync(keyword);
            
        return Json(users); 
    }

    public IActionResult Friends_request()
    {
        return View();
    }

    public IActionResult Profiles()
    {
        return View();
    }
    
    [HttpPost] // ระบุว่าเป็น POST
    public async Task<IActionResult> DeleteFriend(string targetUserId)
    {
        // ดึง ID ของเราจาก Cookie (จากที่ทำ Mock Login ไว้) 
        // หรือถ้าไม่ได้ทำไว้ ให้ fix เป็น ProSniper99 ไปก่อน
        string currentUserId = Request.Cookies["CurrentUserId"] ?? "65d8a0b1c2d3e4f5a6b7c8d1"; 

        // สั่งลบเพื่อนใน Service
        bool success = await _userService.RemoveFriendAsync(currentUserId, targetUserId);
        
        if (success)
        {
            return Json(new { success = true, message = "ลบเพื่อนสำเร็จแล้ว" });
        }
        else
        {
            return BadRequest(Json(new { success = false, message = "ลบเพื่อนไม่สำเร็จ หรือไม่ได้เป็นเพื่อนกันอยู่แล้ว" }));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUserDetails(string userId)
    {
        string currentUserId = Request.Cookies["CurrentUserId"] ?? "65d8a0b1c2d3e4f5a6b7c8d1"; 
        var user = await _userService.SearchUserAsync(userId);
        var me = await _userService.SearchUserAsync(currentUserId);
        if (user == null)
        {
            return NotFound();
        }
        bool isAlreadyFriend = me?.FriendIds?.Contains(userId) ?? false;
       
        var result = new 
    {
        id = user.Id,
        username = user.Username,
        bio = user.Bio,
        avatar = user.Avatar,
        status = user.VibeTags.FirstOrDefault() ?? "Online",
        isFriend = isAlreadyFriend
    };

        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> SendFriendRequest(string targetUserId)
    {
        string currentUserId = Request.Cookies["CurrentUserId"] ?? "65d8a0b1c2d3e4f5a6b7c8d1"; 
        Console.WriteLine($"[DEBUG] พยายามส่งคำขอจาก: {currentUserId} ไปหา: {targetUserId}");
        bool success = await _friendRequestService.SendRequestAsync(currentUserId, targetUserId);
        
        if (success) return Json(new { success = true, message = "ส่งคำขอเป็นเพื่อนแล้ว!" });
        return BadRequest(Json(new { success = false, message = "ไม่สามารถส่งคำขอได้ (อาจส่งไปแล้ว)" }));
    }

[HttpPost]
public async Task<IActionResult> AcceptFriendRequest(string requestId)
{
    bool success = await _friendRequestService.AcceptRequestAsync(requestId);
    
    if (success) return Json(new { success = true, message = "ยอมรับคำขอแล้ว! ตอนนี้คุณเป็นเพื่อนกันแล้ว" });
    return BadRequest(Json(new { success = false, message = "เกิดข้อผิดพลาดในการยอมรับคำขอ" }));
}



// สร้าง URL พิเศษไว้สำหรับสลับบัญชีชั่วคราว
    [HttpGet]
    public IActionResult MockLogin(string id)
    {
        // สั่งบันทึก ID ลงใน Cookie ของเบราว์เซอร์นั้นๆ
        Response.Cookies.Append("CurrentUserId", id, new CookieOptions { Expires = DateTime.Now.AddDays(1) });
        return Content($"✅ ล็อกอินสำเร็จ! ตอนนี้คุณสวมรอยเป็น User ID: {id} แล้ว (ลองกลับไปหน้าเว็บหลักเพื่อเทสระบบเพื่อนได้เลย)");
    }





}
