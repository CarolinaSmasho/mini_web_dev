using Microsoft.AspNetCore.Mvc;

using GamerLFG.Models;
using GamerLFG.Repositories;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IEndorsementRepository _endorsementRepository;
        private readonly KarmaService _karmaService;

        public UserController(IUserRepository userRepository, IEndorsementRepository endorsementRepository, KarmaService karmaService)
        {
            _userRepository = userRepository;
            _endorsementRepository = endorsementRepository;
            _karmaService = karmaService;
        }

        // GET: User/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            var user = await _userRepository.GetUserAsync(userId);
            if (user == null) return NotFound();

            // Get karma history for the user
            var karmaHistory = await _karmaService.GetKarmaHistoryAsync(userId, 20);
            ViewData["KarmaHistory"] = karmaHistory;

            ViewData["Title"] = "My Profile";
            return View(user);
        }

        // GET: User/Friends
        public async Task<IActionResult> Friends()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            var user = await _userRepository.GetUserAsync(userId);
            if (user == null) return NotFound();

            var friends = new List<User>();
            if (user.FriendIds != null && user.FriendIds.Any())
            {
                friends = await _userRepository.GetUsersAsync(user.FriendIds);
            }

            ViewData["Title"] = "Friends";
            return View(friends);
        }

        // GET: User/FriendProfile/{username}
        public async Task<IActionResult> FriendProfile(string id)
        {
            var user = await _userRepository.GetUserByUsernameAsync(id);
            if (user == null) return NotFound();

            // Get karma history for viewing other user's profile (public)
            var karmaHistory = await _karmaService.GetKarmaHistoryAsync(user.Id, 20);
            ViewData["KarmaHistory"] = karmaHistory;

            ViewData["Title"] = "User Profile";
            ViewData["Username"] = user.Username;
            return View("Profile", user); // Reuse Profile view but for another user
        }

        // GET: User/Settings
        public async Task<IActionResult> Settings()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            var user = await _userRepository.GetUserAsync(userId);
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
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userRepository.GetUserAsync(userId);
            if (user == null) return NotFound();

            // Update allowed fields
            if (!string.IsNullOrEmpty(updatedUser.Username)) user.Username = updatedUser.Username;
            user.Bio = updatedUser.Bio;
            
            if (!string.IsNullOrEmpty(gamesPlayedInput))
            {
                user.GameLibrary = gamesPlayedInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(g => g.Trim())
                                                   .ToList();
            }

            user.DiscordUserId = updatedUser.DiscordUserId;
            user.SteamId = updatedUser.SteamId;
            user.TwitchChannel = updatedUser.TwitchChannel;

            await _userRepository.UpdateUserAsync(user);

            TempData["Success"] = "Profile updated!";
            return RedirectToAction("Profile");
        }

        // GET: User/KarmaHistory/{userId} - Public endpoint
        public async Task<IActionResult> KarmaHistory(string id)
        {
            var user = await _userRepository.GetUserAsync(id);
            if (user == null) return NotFound();

            var karmaHistory = await _karmaService.GetKarmaHistoryAsync(id, 50);
            
            ViewData["Title"] = $"{user.Username}'s Karma History";
            ViewData["TargetUser"] = user;
            return View(karmaHistory);
        }
    }
}
