using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services.Interface;
using GamerLFG.Services.Interface.DTOs;
namespace GamerLFG.Controllers;
using System.Security.Claims;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ILobbyService _lobbyService; // เพิ่ม Service นี้เข้ามา

    // ฉีดเข้ามาใน Constructor
    public HomeController(ILogger<HomeController> logger, ILobbyService lobbyService)
    {
        _logger = logger;
        _lobbyService = lobbyService;
    }
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var allLob =  await _lobbyService.GetAllLobbyAsync(userId);
        return View(allLob);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    // public IActionResult Login()
    // {
    //     return View();
    // }

    // public IActionResult Register()
    // {
    //     return View();
    // }

    public IActionResult Profiles()
    {
        return View();
    }

    public IActionResult KarmaHistory()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    
    
}
