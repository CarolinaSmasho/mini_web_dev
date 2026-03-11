using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;

using System.Threading.Tasks;

using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

using System.Runtime.Serialization;
using Microsoft.AspNetCore.Identity;

namespace GamerLFG.Controllers;

public class AuthController : Controller
{   
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {

        _authService = authService;
    }
    public IActionResult Register()
    {   
        ModelState.Clear();
        return View();
    }

        
    [HttpPost]
public async Task<IActionResult> Register(string name, string email, string username, string password)
{

    if (await _authService.IsUserExists(username, email))
    {
        ModelState.AddModelError("", "Username or Email already exists.");
        return View();
    }

    var newUser = new User
    {
        Name = name,
        Email = email,
        Username = username,
        Avatar = "https://static.vecteezy.com/system/resources/thumbnails/009/292/244/small/default-avatar-icon-of-social-media-user-vector.jpg"
    };

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

    var user = await _authService.VerifyLoginAsync(username,password);

    if(user == null)
        {
            ModelState.AddModelError("","Invalid Username or Password");
            return View();
        }

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("FullName", user.Name ?? "")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return RedirectToAction("Index", "Home");
    }
    
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}