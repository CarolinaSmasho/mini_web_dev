using Microsoft.AspNetCore.Mvc;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LobbyApiController : ControllerBase
    {
        private readonly ILobbyService _lobbyService;
        private readonly IUserService _userService;

        public LobbyApiController(ILobbyService lobbyService, IUserService userService)
        {
            _lobbyService = lobbyService;
            _userService = userService;
        }

        /// <summary>
        /// Create a new lobby
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLobby([FromBody] CreateLobbyRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId))
                return BadRequest(new { success = false, error = "HostUserId is required" });

            var lobby = new GamerLFG.Models.Lobby
            {
                Title = request.Title,
                Game = request.Game,
                Description = request.Description,
                MaxPlayers = request.MaxPlayers,
                RecruitmentDeadline = request.RecruitmentDeadline,
                SessionStartTime = request.SessionStartTime,
                SessionEndTime = request.SessionEndTime,
                Status = "Active",
                IsRecruiting = true,
                PictureUrl = request.PictureUrl ?? "https://images.unsplash.com/photo-1542751371-adc38448a05e?q=80&w=2070&auto=format&fit=crop"
            };

            var roleNames = request.Roles?.Select(r => r.Name).ToList() ?? new List<string>();
            var roleCounts = request.Roles?.Select(r => r.Count).ToList() ?? new List<int>();

            var lobbyId = await _lobbyService.CreateLobbyAsync(request.HostUserId, lobby, request.Moods ?? new List<string>(), roleNames, roleCounts, request.HostRole);

            return CreatedAtAction(nameof(GetLobby), new { id = lobbyId }, new { success = true, lobbyId });
        }

        /// <summary>
        /// Update an existing lobby (Host only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLobby(string id, [FromBody] UpdateLobbyRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId))
                return BadRequest(new { success = false, error = "HostUserId is required" });

            var existingLobby = await _lobbyService.GetLobbyAsync(id);
            if (existingLobby == null) return NotFound(new { success = false, error = "Lobby not found" });
            if (existingLobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            var updatedLobby = new GamerLFG.Models.Lobby
            {
                Title = request.Title ?? existingLobby.Title,
                Game = request.Game ?? existingLobby.Game,
                Description = request.Description ?? existingLobby.Description,
                MaxPlayers = request.MaxPlayers > 0 ? request.MaxPlayers : existingLobby.MaxPlayers,
                PictureUrl = request.PictureUrl ?? existingLobby.PictureUrl,
                DiscordLink = request.DiscordLink ?? existingLobby.DiscordLink,
                RecruitmentDeadline = request.RecruitmentDeadline,
                SessionStartTime = request.SessionStartTime,
                SessionEndTime = request.SessionEndTime
            };

            var roleNames = request.Roles?.Select(r => r.Name).ToList() ?? new List<string>();
            var roleCounts = request.Roles?.Select(r => r.Count).ToList() ?? new List<int>();

            await _lobbyService.UpdateLobbyAsync(id, request.HostUserId, updatedLobby, request.Moods ?? new List<string>(), roleNames, roleCounts);
            return Ok(new { success = true, message = "Lobby updated" });
        }

        /// <summary>
        /// Get all lobbies with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLobbies([FromQuery] string? game = null, [FromQuery] string? mood = null, [FromQuery] int page = 1)
        {
            var lobbies = await _lobbyService.GetLobbiesAsync(game);
            return Ok(new { success = true, data = new { lobbies, pagination = new { currentPage = page, totalPages = 1 } } });
        }

        /// <summary>
        /// Get a specific lobby by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLobby(string id)
        {
            var lobby = await _lobbyService.GetLobbyAsync(id);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            var host = await _userService.GetUserAsync(lobby.HostId);
            return Ok(new { success = true, lobby, hostName = host?.Username });
        }

        /// <summary>
        /// Get applications for a lobby
        /// </summary>
        [HttpGet("{id}/applications")]
        public async Task<IActionResult> GetApplications(string id)
        {
            var applications = await _lobbyService.GetApplicationsByLobbyIdAsync(id);
            return Ok(new { success = true, applications });
        }

        /// <summary>
        /// Apply to a lobby
        /// </summary>
        [HttpPost("{id}/apply")]
        public async Task<IActionResult> Apply(string id, [FromBody] ApplyRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { success = false, error = "UserId is required" });

            var (success, error) = await _lobbyService.ApplyAsync(id, request.UserId, request.Role);
            if (!success) return BadRequest(new { success = false, error });
            return Ok(new { success = true, message = "Application sent" });
        }

        /// <summary>
        /// Recruit an applicant (Host only)
        /// </summary>
        [HttpPost("applications/{applicationId}/recruit")]
        public async Task<IActionResult> Recruit(string applicationId, [FromBody] HostActionRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId))
                return BadRequest(new { success = false, error = "HostUserId is required" });

            var (success, error) = await _lobbyService.RecruitAsync(applicationId, request.HostUserId);
            if (!success)
            {
                if (error == "Not authorized") return Unauthorized(new { success = false, error });
                if (error == "Application not found" || error == "Lobby not found") return NotFound(new { success = false, error });
                return BadRequest(new { success = false, error });
            }
            return Ok(new { success = true, message = "Applicant recruited" });
        }

        /// <summary>
        /// Reject an applicant (Host only)
        /// </summary>
        [HttpPost("applications/{applicationId}/reject")]
        public async Task<IActionResult> Reject(string applicationId, [FromBody] HostActionRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId))
                return BadRequest(new { success = false, error = "HostUserId is required" });

            var (success, error) = await _lobbyService.RejectAsync(applicationId, request.HostUserId);
            if (!success)
            {
                if (error == "Not authorized") return Unauthorized(new { success = false, error });
                return NotFound(new { success = false, error });
            }
            return Ok(new { success = true, message = "Applicant rejected" });
        }

        /// <summary>
        /// Kick a member from lobby (Host only)
        /// </summary>
        [HttpPost("{id}/kick")]
        public async Task<IActionResult> Kick(string id, [FromBody] KickRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId) || string.IsNullOrEmpty(request.MemberId))
                return BadRequest(new { success = false, error = "HostUserId and MemberId are required" });

            var (success, error) = await _lobbyService.KickAsync(id, request.MemberId, request.HostUserId, request.HardKick);
            if (!success)
            {
                if (error == "Not authorized") return Unauthorized(new { success = false, error });
                if (error == "Lobby not found") return NotFound(new { success = false, error });
                return BadRequest(new { success = false, error });
            }
            return Ok(new { success = true, hardKick = request.HardKick });
        }

        /// <summary>
        /// Toggle recruitment status (Host only)
        /// </summary>
        [HttpPost("{id}/toggle-recruitment")]
        public async Task<IActionResult> ToggleRecruitment(string id, [FromBody] HostActionRequest request)
        {
            var (success, isRecruiting, error) = await _lobbyService.ToggleRecruitmentAsync(id, request.HostUserId);
            if (!success) return Unauthorized(new { success = false, error });
            return Ok(new { success = true, isRecruiting });
        }

        /// <summary>
        /// Complete a lobby session (Host only)
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteSession(string id, [FromBody] HostActionRequest request)
        {
            var (success, error) = await _lobbyService.CompleteSessionAsync(id, request.HostUserId);
            if (!success) return Unauthorized(new { success = false, error });
            return Ok(new { success = true, message = "Session completed, karma awarded" });
        }

        /// <summary>
        /// Update lobby background image (Host only)
        /// </summary>
        [HttpPut("{id}/background")]
        public async Task<IActionResult> UpdateBackground(string id, [FromBody] BackgroundUpdateRequest request)
        {
            var (success, error) = await _lobbyService.UpdateBackgroundAsync(id, request.PictureUrl, request.HostUserId);
            if (!success) return Unauthorized(new { success = false, error });
            return Ok(new { success = true });
        }

        /// <summary>
        /// Endorse a member after session completion
        /// </summary>
        [HttpPost("{id}/endorse")]
        public async Task<IActionResult> EndorseMember(string id, [FromBody] EndorseRequest request)
        {
            var (success, error) = await _lobbyService.EndorseMemberAsync(id, request.FromUserId, request.TargetUserId, request.Type);
            if (!success) return BadRequest(new { success = false, error });
            return Ok(new { success = true });
        }

        /// <summary>
        /// Delete a lobby (Host only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string hostUserId)
        {
            var lobby = await _lobbyService.GetLobbyAsync(id);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });
            if (lobby.HostId != hostUserId) return Unauthorized(new { success = false, error = "Not authorized" });

            await _lobbyService.DeleteLobbyAsync(id);
            return Ok(new { success = true, message = "Lobby deleted" });
        }
    }

    // Request DTOs
    public class CreateLobbyRequest
    {
        public string HostUserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Game { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxPlayers { get; set; } = 4;
        public List<string>? Moods { get; set; }
        public List<RoleRequest>? Roles { get; set; }
        public DateTime? RecruitmentDeadline { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }
        public string? PictureUrl { get; set; }
        public string? HostRole { get; set; }
    }

    public class RoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class UpdateLobbyRequest
    {
        public string HostUserId { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Game { get; set; }
        public string? Description { get; set; }
        public int MaxPlayers { get; set; }
        public List<string>? Moods { get; set; }
        public List<RoleRequest>? Roles { get; set; }
        public DateTime? RecruitmentDeadline { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }
        public string? PictureUrl { get; set; }
        public string? DiscordLink { get; set; }
    }

    public class ApplyRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class HostActionRequest
    {
        public string HostUserId { get; set; } = string.Empty;
    }

    public class KickRequest
    {
        public string HostUserId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public bool HardKick { get; set; } = false;
    }

    public class BackgroundUpdateRequest
    {
        public string HostUserId { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
    }

    public class EndorseRequest
    {
        public string FromUserId { get; set; } = string.Empty;
        public string TargetUserId { get; set; } = string.Empty;
        public string Type { get; set; } = "Positive";
    }
}
