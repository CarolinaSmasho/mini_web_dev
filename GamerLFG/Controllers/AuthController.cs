using Microsoft.AspNetCore.Mvc;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var user = await _authService.ValidateLoginAsync(email, password);
                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Email", user.Email);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewData["Error"] = "Invalid email or password";
            return View();
        }

        // GET: /Auth/Register
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("UserId") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            var (success, error, _) = await _authService.RegisterAsync(username, email, password, confirmPassword);
            if (!success)
            {
                ViewData["Error"] = error;
                return View();
            }

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
