using Microsoft.AspNetCore.Mvc;

using GamerLFG.Models;
using GamerLFG.Repositories;

namespace GamerLFG.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepository;

        public AuthController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

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
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                
                if (user != null && user.Password == password)
                {
                    // Set session
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

            var existingUser = await _userRepository.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                ViewData["Error"] = "Email already registered";
                return View();
            }

            var newUser = new User 
            { 
                Username = username, 
                Email = email, 
                Password = password 
            };
            
            await _userRepository.CreateUserAsync(newUser);

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
