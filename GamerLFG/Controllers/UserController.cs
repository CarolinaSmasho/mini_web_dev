using Microsoft.AspNetCore.Mvc;

namespace GamerLFG.Controllers
{
    public class UserController : Controller
    {
        // GET: User/Profile
        public IActionResult Profile()
        {
            // Check if user is logged in
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            ViewData["Title"] = "My Profile";
            return View();
        }

        // GET: User/Friends
        public IActionResult Friends()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            ViewData["Title"] = "Friends";
            return View();
        }

        // GET: User/FriendProfile/{username}
        public IActionResult FriendProfile(string id)
        {
            ViewData["Title"] = "User Profile";
            ViewData["Username"] = id ?? "Unknown";
            return View();
        }

        // GET: User/Settings
        public IActionResult Settings()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            
            ViewData["Title"] = "Settings";
            return View();
        }
    }
}
