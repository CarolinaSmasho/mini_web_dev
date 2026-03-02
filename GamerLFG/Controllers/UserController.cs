using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;
using MongoDB.Driver;
using GamerLFG.service;
using System.Security.Claims;

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
       
        bool isPendingRequest = false;
        if (!isAlreadyFriend)
        {
            isPendingRequest = await _friendRequestService.HasPendingRequestAsync(currentUserId, userId);
        }

        var result = new 
        {
            id = user.Id,
            username = user.Username,
            bio = user.Bio,
            avatar = user.Avatar,
            status = user.VibeTags.FirstOrDefault() ?? "Online",
            isFriend = isAlreadyFriend,
            isPending = isPendingRequest
        };

        return Json(result);
    }
    
    private readonly MongoDBservice _mongoDBservice;


// สร้าง URL พิเศษไว้สำหรับสลับบัญชีชั่วคราว
    [HttpGet]
    public IActionResult MockLogin(string id)
    {
        // สั่งบันทึก ID ลงใน Cookie ของเบราว์เซอร์นั้นๆ
        Response.Cookies.Append("CurrentUserId", id, new CookieOptions { Expires = DateTime.Now.AddDays(1) });
        return Content($"✅ ล็อกอินสำเร็จ! ตอนนี้คุณสวมรอยเป็น User ID: {id} แล้ว (ลองกลับไปหน้าเว็บหลักเพื่อเทสระบบเพื่อนได้เลย)");
    }



    public UserController(MongoDBservice mongoDBservice)
    {
        _mongoDBservice = mongoDBservice;
    }

    [HttpGet]
    public async Task<IActionResult> Profiles(string id)
    {
        if (string.IsNullOrEmpty(id)) 
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Login", "Auth");
        }

        var user = await _mongoDBservice.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();

        if (user == null) return NotFound();

        var karmaRecords = await _mongoDBservice.KarmaHistories
            .Find(k => k.TargetUserId == id)
            .ToListAsync();

        user.KarmaScore = karmaRecords.Sum(k => k.Score); 

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(string id, string Username, string Bio, string Avatar,string GameLibraryString, string VibeTagsString, string discord, string steam, string twitch)
    {
        var user = await _mongoDBservice.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null) return NotFound();

        user.Username = Username;
        user.Avatar = Avatar;
        user.Bio = Bio;
        user.discord = discord;
        user.steam = steam;
        user.twitch = twitch;

        if (!string.IsNullOrEmpty(GameLibraryString))
        {
            user.GameLibrary = GameLibraryString.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();
        }

        if (!string.IsNullOrEmpty(VibeTagsString))
        {
            user.VibeTags = VibeTagsString.Split(',')
                .Select(v => v.Trim())
                .ToList();
        }
        else
        {
            user.VibeTags = new List<string>();
        }

        await _mongoDBservice.Users.ReplaceOneAsync(u => u.Id == id, user);

        return RedirectToAction("Profiles", new { id = id });
    }
}
