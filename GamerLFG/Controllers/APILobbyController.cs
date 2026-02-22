using Microsoft.AspNetCore.Mvc;
using GamerLFG.Services.Interface.DTOs; // เปลี่ยนเป็น Namespace ที่เก็บ CreateLobbyDTO ของคุณ
using GamerLFG.Services;

[ApiController] // สำคัญมาก: เพื่อให้ Swagger รู้ว่าเป็น API
[Route("api/[controller]")] // URL จะเป็น /api/lobby
public class LobbyController : ControllerBase
{
    private readonly LobbyService _lobbyService;

    public LobbyController(LobbyService lobbyService)
    {
        _lobbyService = lobbyService;
    }

    [HttpPost("create")] // ระบุ Http Method และ Path
    public async Task<IActionResult> CreateLobby([FromBody] CreateLobbyDTO newLobby)
    {
        try 
        {
            await _lobbyService.CreateLobbyAsync(newLobby);
            return Ok(new { message = "Lobby created successfully!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}