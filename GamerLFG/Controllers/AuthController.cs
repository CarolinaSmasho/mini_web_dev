using Microsoft.AspNetCore.Mvc;

namespace GamerLFG.Controllers
{
    public class AuthController : Controller
    {
        // GET: /Auth/Login
        public IActionResult Login()
        {
            // If already logged in, redirect to home
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // TODO: Implement Firebase authentication
            // For now, create a mock session
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                // Mock user session (replace with Firebase auth)
                HttpContext.Session.SetString("UserId", Guid.NewGuid().ToString());
                HttpContext.Session.SetString("Username", email.Split('@')[0]);
                HttpContext.Session.SetString("Email", email);
                
                return RedirectToAction("Index", "Home");
            }
            
            ViewData["Error"] = "Invalid email or password";
            return View();
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            // Validation
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || 
                string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                ViewData["Error"] = "Please fill all fields correctly";
                return View();
            }

            // TODO: Implement Firebase user creation
            // For now, redirect to login
            TempData["Success"] = "Account created successfully! Please log in.";
            return RedirectToAction("Login");
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        // GET: /Auth/Logout (for convenience)
        public IActionResult LogoutGet()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
