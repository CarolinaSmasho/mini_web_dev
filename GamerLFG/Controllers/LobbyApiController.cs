using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Repositories;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LobbyApiController : ControllerBase
    {
        private readonly ILobbyRepository _lobbyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IEndorsementRepository _endorsementRepository;
        private readonly KarmaService _karmaService;

        public LobbyApiController(
            ILobbyRepository lobbyRepository, 
            IUserRepository userRepository,
            IApplicationRepository applicationRepository, 
            IEndorsementRepository endorsementRepository,
            KarmaService karmaService)
        {
            _lobbyRepository = lobbyRepository;
            _userRepository = userRepository;
            _applicationRepository = applicationRepository;
            _endorsementRepository = endorsementRepository;
            _karmaService = karmaService;
        }

        /// <summary>
        /// Create a new lobby
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLobby([FromBody] CreateLobbyRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId))
                return BadRequest(new { success = false, error = "HostUserId is required" });

            var lobby = new Lobby
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
                Moods = request.Moods ?? new List<string>(),
                PictureUrl = request.PictureUrl ?? "https://images.unsplash.com/photo-1542751371-adc38448a05e?q=80&w=2070&auto=format&fit=crop"
            };

            // Map Roles
            lobby.Roles = new List<Role>();
            int totalRolePlayers = 0;
            if (request.Roles != null)
            {
                foreach (var r in request.Roles)
                {
                    if (!string.IsNullOrWhiteSpace(r.Name) && r.Count > 0)
                    {
                        int count = r.Count;
                        if (totalRolePlayers + count > lobby.MaxPlayers)
                            count = lobby.MaxPlayers - totalRolePlayers;

                        if (count > 0)
                        {
                            lobby.Roles.Add(new Role { Name = r.Name, Count = count, Filled = 0 });
                            totalRolePlayers += count;
                        }
                    }
                    if (totalRolePlayers >= lobby.MaxPlayers) break;
                }
            }

            if (totalRolePlayers < lobby.MaxPlayers)
            {
                lobby.Roles.Add(new Role { Name = "Other", Count = lobby.MaxPlayers - totalRolePlayers, Filled = 0 });
            }

            lobby.HostId = request.HostUserId;
            lobby.Members = new List<Member>
            {
                new Member 
                { 
                    UserId = request.HostUserId, 
                    AssignedRole = lobby.Roles.FirstOrDefault()?.Name ?? "Host", 
                    IsHost = true, 
                    JoinedAt = DateTime.UtcNow 
                }
            };

            var hostRole = lobby.Roles.FirstOrDefault(r => r.Name == lobby.Members[0].AssignedRole);
            if (hostRole != null) hostRole.Filled = 1;

            if (lobby.RecruitmentDeadline.HasValue) 
                lobby.RecruitmentDeadline = lobby.RecruitmentDeadline.Value.ToUniversalTime();
            if (lobby.SessionStartTime.HasValue)
                lobby.SessionStartTime = lobby.SessionStartTime.Value.ToUniversalTime();
            if (lobby.SessionEndTime.HasValue)
                lobby.SessionEndTime = lobby.SessionEndTime.Value.ToUniversalTime();

            lobby.CreatedAt = DateTime.UtcNow;
            await _lobbyRepository.CreateLobbyAsync(lobby);

            return CreatedAtAction(nameof(GetLobby), new { id = lobby.Id }, new { success = true, lobbyId = lobby.Id });
        }

        /// <summary>
        /// Update an existing lobby (Host only)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLobby(string id, [FromBody] UpdateLobbyRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId))
                return BadRequest(new { success = false, error = "HostUserId is required" });

            var existingLobby = await _lobbyRepository.GetLobbyAsync(id);
            if (existingLobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            if (existingLobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            // Update basic fields
            if (!string.IsNullOrEmpty(request.Title)) existingLobby.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Game)) existingLobby.Game = request.Game;
            if (request.Description != null) existingLobby.Description = request.Description;
            if (request.MaxPlayers > 0) existingLobby.MaxPlayers = request.MaxPlayers;
            if (!string.IsNullOrEmpty(request.PictureUrl)) existingLobby.PictureUrl = request.PictureUrl;
            if (request.DiscordLink != null) existingLobby.DiscordLink = request.DiscordLink;
            
            if (request.RecruitmentDeadline.HasValue)
                existingLobby.RecruitmentDeadline = request.RecruitmentDeadline.Value.ToUniversalTime();
            if (request.SessionStartTime.HasValue)
                existingLobby.SessionStartTime = request.SessionStartTime.Value.ToUniversalTime();
            if (request.SessionEndTime.HasValue)
                existingLobby.SessionEndTime = request.SessionEndTime.Value.ToUniversalTime();

            if (request.Moods != null) existingLobby.Moods = request.Moods;

            // Handle Roles update if provided
            if (request.Roles != null)
            {
                existingLobby.Roles = new List<Role>();
                int totalRolePlayers = 0;
                foreach (var r in request.Roles)
                {
                    if (!string.IsNullOrWhiteSpace(r.Name) && r.Count > 0)
                    {
                        int count = r.Count;
                        if (totalRolePlayers + count > existingLobby.MaxPlayers)
                            count = existingLobby.MaxPlayers - totalRolePlayers;

                        if (count > 0)
                        {
                            existingLobby.Roles.Add(new Role { Name = r.Name, Count = count, Filled = 0 });
                            totalRolePlayers += count;
                        }
                    }
                    if (totalRolePlayers >= existingLobby.MaxPlayers) break;
                }

                if (totalRolePlayers < existingLobby.MaxPlayers)
                {
                    existingLobby.Roles.Add(new Role { Name = "Other", Count = existingLobby.MaxPlayers - totalRolePlayers, Filled = 0 });
                }

                // Recalculate filled roles based on current members
                foreach (var role in existingLobby.Roles)
                {
                    role.Filled = existingLobby.Members.Count(m => m.AssignedRole == role.Name);
                }
            }

            await _lobbyRepository.UpdateLobbyAsync(existingLobby);
            return Ok(new { success = true, message = "Lobby updated" });
        }

        /// <summary>
        /// Get all lobbies with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLobbies([FromQuery] string? game = null, [FromQuery] string? mood = null, [FromQuery] int page = 1)
        {
            var lobbies = await _lobbyRepository.GetLobbiesAsync();
            
            if (!string.IsNullOrEmpty(game))
            {
                lobbies = lobbies.Where(l => l.Game.Contains(game, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Ok(new { success = true, data = new { lobbies, pagination = new { currentPage = page, totalPages = 1 } } });
        }

        /// <summary>
        /// Get a specific lobby by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLobby(string id)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            var host = await _userRepository.GetUserAsync(lobby.HostId);
            return Ok(new { success = true, lobby, hostName = host?.Username });
        }

        /// <summary>
        /// Get applications for a lobby
        /// </summary>
        [HttpGet("{id}/applications")]
        public async Task<IActionResult> GetApplications(string id)
        {
            var applications = await _applicationRepository.GetApplicationsByLobbyIdAsync(id);
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

            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            if (lobby.RecruitmentDeadline.HasValue && DateTime.UtcNow > lobby.RecruitmentDeadline.Value)
                return BadRequest(new { success = false, error = "Recruitment period has ended" });

            if (lobby.Members.Any(m => m.UserId == request.UserId))
                return BadRequest(new { success = false, error = "Already a member" });

            var existingApps = await _applicationRepository.GetApplicationsByLobbyIdAsync(id);
            if (existingApps.Any(a => a.UserId == request.UserId && a.Status == "Pending"))
                return BadRequest(new { success = false, error = "Application already pending" });

            var application = new Application
            {
                LobbyId = id,
                UserId = request.UserId,
                DesiredRoles = !string.IsNullOrEmpty(request.Role) ? new List<string> { request.Role } : new List<string>(),
                Message = string.IsNullOrEmpty(request.Role) ? "Requesting to join" : $"Applying for {request.Role}",
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };

            await _applicationRepository.CreateApplicationAsync(application);
            return Ok(new { success = true, message = "Application sent", applicationId = application.Id });
        }

        /// <summary>
        /// Recruit an applicant (Host only)
        /// </summary>
        [HttpPost("applications/{applicationId}/recruit")]
        public async Task<IActionResult> Recruit(string applicationId, [FromBody] HostActionRequest request)
        {
            if (string.IsNullOrEmpty(request.HostUserId))
                return BadRequest(new { success = false, error = "HostUserId is required" });

            var application = await _applicationRepository.GetApplicationAsync(applicationId);
            if (application == null) return NotFound(new { success = false, error = "Application not found" });

            var lobby = await _lobbyRepository.GetLobbyAsync(application.LobbyId);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            if (lobby.Members.Count >= lobby.MaxPlayers)
                return BadRequest(new { success = false, error = "Lobby is full" });

            await _applicationRepository.UpdateApplicationStatusAsync(applicationId, "Accepted");

            var role = application.DesiredRoles?.FirstOrDefault() ?? "Member";
            var member = new Member
            {
                UserId = application.UserId,
                AssignedRole = role,
                IsHost = false,
                JoinedAt = DateTime.UtcNow
            };

            await _lobbyRepository.AddMemberAsync(lobby.Id, member);
            await _karmaService.AwardLobbyAcceptedAsync(application.UserId, lobby.Id, lobby.Title);

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

            var application = await _applicationRepository.GetApplicationAsync(applicationId);
            if (application == null) return NotFound(new { success = false, error = "Application not found" });

            var lobby = await _lobbyRepository.GetLobbyAsync(application.LobbyId);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            await _applicationRepository.UpdateApplicationStatusAsync(applicationId, "Rejected");
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

            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            if (request.MemberId == lobby.HostId)
                return BadRequest(new { success = false, error = "Cannot kick host" });

            await _lobbyRepository.RemoveMemberAsync(id, request.MemberId);
            await _karmaService.PenalizeKickedAsync(request.MemberId, id, lobby.Title, request.HardKick);

            return Ok(new { success = true, hardKick = request.HardKick });
        }

        /// <summary>
        /// Toggle recruitment status (Host only)
        /// </summary>
        [HttpPost("{id}/toggle-recruitment")]
        public async Task<IActionResult> ToggleRecruitment(string id, [FromBody] HostActionRequest request)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || lobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Unauthorized" });

            lobby.IsRecruiting = !lobby.IsRecruiting;
            await _lobbyRepository.UpdateLobbyAsync(lobby);

            return Ok(new { success = true, isRecruiting = lobby.IsRecruiting });
        }

        /// <summary>
        /// Complete a lobby session (Host only)
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> CompleteSession(string id, [FromBody] HostActionRequest request)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || lobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Unauthorized" });

            lobby.IsCompleted = true;
            lobby.Status = "Closed";
            await _lobbyRepository.UpdateLobbyAsync(lobby);

            // Award karma
            await _karmaService.AwardLobbyCompletedHostAsync(lobby.HostId, lobby.Id, lobby.Title);
            foreach (var member in lobby.Members.Where(m => !m.IsHost))
            {
                await _karmaService.AwardLobbyCompletedMemberAsync(member.UserId, lobby.Id, lobby.Title);
            }

            return Ok(new { success = true, message = "Session completed, karma awarded" });
        }

        /// <summary>
        /// Update lobby background image (Host only)
        /// </summary>
        [HttpPut("{id}/background")]
        public async Task<IActionResult> UpdateBackground(string id, [FromBody] BackgroundUpdateRequest request)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || lobby.HostId != request.HostUserId)
                return Unauthorized(new { success = false, error = "Unauthorized" });

            lobby.PictureUrl = request.PictureUrl;
            await _lobbyRepository.UpdateLobbyAsync(lobby);

            return Ok(new { success = true });
        }

        /// <summary>
        /// Endorse a member after session completion
        /// </summary>
        [HttpPost("{id}/endorse")]
        public async Task<IActionResult> EndorseMember(string id, [FromBody] EndorseRequest request)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null || !lobby.IsCompleted)
                return BadRequest(new { success = false, error = "Session not completed" });

            if (!lobby.Members.Any(m => m.UserId == request.FromUserId) || 
                !lobby.Members.Any(m => m.UserId == request.TargetUserId))
                return BadRequest(new { success = false, error = "Invalid members" });

            var endorsement = new Endorsement
            {
                FromUserId = request.FromUserId,
                ToUserId = request.TargetUserId,
                EndorsementType = request.Type,
                Comment = "Session Feedback",
                CreatedAt = DateTime.UtcNow
            };

            await _endorsementRepository.CreateEndorsementAsync(endorsement);

            var fromUser = await _userRepository.GetUserAsync(request.FromUserId);
            await _karmaService.ProcessEndorsementAsync(
                request.TargetUserId,
                fromUser?.Username ?? "Unknown",
                request.Type,
                endorsement.Id
            );

            return Ok(new { success = true, endorsementId = endorsement.Id });
        }

        /// <summary>
        /// Delete a lobby (Host only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery] string hostUserId)
        {
            var lobby = await _lobbyRepository.GetLobbyAsync(id);
            if (lobby == null) return NotFound(new { success = false, error = "Lobby not found" });

            if (lobby.HostId != hostUserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            await _lobbyRepository.DeleteLobbyAsync(id);
            await _applicationRepository.DeleteApplicationsByLobbyIdAsync(id);

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
