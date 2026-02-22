using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;

namespace GamerLFG.Controllers;

public class AuthController : Controller
{
    public IActionResult Login()
    {
        return View();
    }
}