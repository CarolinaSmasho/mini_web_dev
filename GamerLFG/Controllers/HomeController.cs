using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Repositories;

namespace GamerLFG.Controllers;

public class HomeController : Controller
{
    private readonly ILobbyRepository _lobbyRepository;
    private readonly IUserRepository _userRepository;
    
    public HomeController(ILobbyRepository lobbyRepository, IUserRepository userRepository)
    {
        _lobbyRepository = lobbyRepository;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index()
    {
        var lobbies = await _lobbyRepository.GetLobbiesAsync();
        
        // Pre-fetch Host names
        var hostIds = lobbies.Select(l => l.HostId).Distinct().ToList();
        var hosts = await _userRepository.GetUsersAsync(hostIds);
        var hostMap = hosts.ToDictionary(u => u.Id, u => u.Username);
        
        ViewData["HostMap"] = hostMap;

        return View(lobbies);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
