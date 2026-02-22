using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly KarmaService _karmaService;

        public UserController(IUserService userService, KarmaService karmaService)
        {
            _userService = userService;
            _karmaService = karmaService;
        }

        // GET: User/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserAsync(userId);
            if (user == null) return NotFound();

            ViewData["KarmaHistory"] = await _karmaService.GetKarmaHistoryAsync(userId, 20);
            ViewData["Title"] = "My Profile";
            return View(user);
        }

        // GET: User/Friends
        public async Task<IActionResult> Friends()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserAsync(userId);
            if (user == null) return NotFound();

            var friends = await _userService.GetFriendsAsync(userId);
            ViewData["Title"] = "Friends";
            return View(friends);
        }

        // GET: User/FriendProfile/{username}
        public async Task<IActionResult> FriendProfile(string id)
        {
            var user = await _userService.GetUserByUsernameAsync(id);
            if (user == null) return NotFound();

            ViewData["KarmaHistory"] = await _karmaService.GetKarmaHistoryAsync(user.Id, 20);
            ViewData["Title"] = "User Profile";
            ViewData["Username"] = user.Username;
            return View("Profile", user);
        }

        // GET: User/Settings
        public async Task<IActionResult> Settings()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _userService.GetUserAsync(userId);
            if (user == null) return NotFound();

            ViewData["Title"] = "Settings";
            return View(user);
        }

        // POST: User/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User updatedUser, string gamesPlayedInput)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            await _userService.UpdateProfileAsync(
                userId,
                updatedUser.Username,
                updatedUser.Bio,
                gamesPlayedInput,
                updatedUser.DiscordUserId,
                updatedUser.SteamId,
                updatedUser.TwitchChannel);

            TempData["Success"] = "Profile updated!";
            return RedirectToAction("Profile");
        }

        // GET: User/KarmaHistory/{id}
        public async Task<IActionResult> KarmaHistory(string id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null) return NotFound();

            var karmaHistory = await _karmaService.GetKarmaHistoryAsync(id, 50);
            ViewData["Title"] = $"{user.Username}'s Karma History";
            ViewData["TargetUser"] = user;
            return View(karmaHistory);
        }
    }
}
