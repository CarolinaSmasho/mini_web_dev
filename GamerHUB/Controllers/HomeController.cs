using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Services;

namespace GamerLFG.Controllers;

public class HomeController : Controller
{
    private readonly HomeService _homeService;

    public HomeController(HomeService homeService)
    {
        _homeService = homeService;
    }
    
    // Get all lobbys
    public async Task<IActionResult> Index()
    {
        var lobbys = await _homeService.GetAllLobbys();
        return View(lobbys);
    }

    public IActionResult Privacy()
    {
        return View();
    }
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

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
