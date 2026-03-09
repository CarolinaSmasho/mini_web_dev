using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;

// Async support
using System.Threading.Tasks;
// For creating claims (identity info)
using System.Security.Claims;
// Authentication support
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;


namespace GamerLFG.Controllers;

public class AuthController : Controller
{   
    private readonly AuthService _authService;

        // Constructor injection
    public AuthController(AuthService authService)
    {
        // Store injected service
        _authService = authService;
    }
    public IActionResult Register()
    {   
        ModelState.Clear();
        return View();
    }
     // Service instance
        
    [HttpPost]
public async Task<IActionResult> Register(string name, string email, string username, string password)
{
    // 1. เช็คว่ามี Username หรือ Email นี้ในระบบหรือยัง
    if (await _authService.IsUserExists(username, email))
    {
        ModelState.AddModelError("", "Username or Email already exists.");
        return View();
    }

    // 2. สร้าง Object User
    var newUser = new User
    {
        Name = name,
        Email = email,
        Username = username,
        Avatar = "https://static.vecteezy.com/system/resources/thumbnails/009/292/244/small/default-avatar-icon-of-social-media-user-vector.jpg"
    };

    //เรียกใช้ Service เพื่อ Hash password และบันทึก
    await _authService.RegisterAsync(newUser, password);

    return RedirectToAction("Login");
}

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
         // Find user by username
    var user = await _authService.VerifyLoginAsync(username,password);

    // Check if user exists AND password matches
    if(user == null)
        {
            ModelState.AddModelError("","Invalid Username or Password");
            return View();
        }

    // Create claims
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id), // เก็บ ID ไว้เพื่อใช้อ้างอิงภายหลัง
        new Claim("FullName", user.Name ?? "")
    };

    // สร้าง Identity โดยใช้ Scheme ให้ตรงกับที่ตั้งค่าใน Program.cs
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    // สั่งให้ระบบออก Cookie ให้กับ Browser
    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}