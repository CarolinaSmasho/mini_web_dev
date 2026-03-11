using Microsoft.AspNetCore.Mvc;
using GamerLFG.Services.Interface.DTOs;
using GamerLFG.Services;
using GamerLFG.Services.Interface;

[ApiController]
[Route("api/[controller]")]
public class LobbyController : ControllerBase
{
    private readonly ILobbyService _lobbyService;

    public LobbyController(ILobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateLobby([FromBody] CreateLobbyDTO newLobby)
    {
        try 
        {
            Console.Write("inner func");
            Console.WriteLine($"Lobby Name: {newLobby.HostId}");
            var (succces,message) = await _lobbyService.CreateLobbyAsync(newLobby);
            if (!succces)return BadRequest(new { error = message });
            return Ok(new { message = "Lobby created successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}