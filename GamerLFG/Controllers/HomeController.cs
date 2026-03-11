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
    private readonly ILobbyService _lobbyService;

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

    [HttpPost]
    public async Task<IActionResult> GetNextLobby([FromBody] ShowLobbyDTO lastLob)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (lastLob == null) return BadRequest("ไม่มีห้องแล้ววว");
        List<ShowLobbyDTO> lobbies = await _lobbyService.GetNextLobbiesAsync(lastLob.Id,userId);
        return PartialView("_LobbyCardsPartial", lobbies);
    }

    [HttpPost]
    public async Task<IActionResult> SearchLobby([FromBody] ShowLobbyDTO target)
    {
        Console.WriteLine(target.HostName);
        Console.WriteLine(target.Title);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (target == null) return BadRequest("ไม่มีห้องที่ถามหา");
        List<ShowLobbyDTO> lobbies = await _lobbyService.GetLobbiesAsyncByName(target.Title,userId,target.HostName);
        ViewBag.HostId = userId;
        return PartialView("_LobbyCardsPartial", lobbies);
    }
 

    public IActionResult Privacy()
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
