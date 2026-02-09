using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Repositories;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEndorsementRepository _endorsementRepository;
        private readonly KarmaService _karmaService;

        public UserApiController(
            IUserRepository userRepository, 
            IEndorsementRepository endorsementRepository, 
            KarmaService karmaService)
        {
            _userRepository = userRepository;
            _endorsementRepository = endorsementRepository;
            _karmaService = karmaService;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userRepository.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            // Don't return password
            return Ok(new { 
                success = true, 
                user = new {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Bio,
                    user.AvatarUrl,
                    user.KarmaScore,
                    user.VibeTags,
                    user.GameLibrary,
                    user.DiscordUserId,
                    user.SteamId,
                    user.TwitchChannel,
                    user.CreatedAt,
                    user.IsOnline
                }
            });
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            return Ok(new { 
                success = true, 
                user = new {
                    user.Id,
                    user.Username,
                    user.Bio,
                    user.AvatarUrl,
                    user.KarmaScore,
                    user.VibeTags,
                    user.GameLibrary,
                    user.DiscordUserId,
                    user.SteamId,
                    user.TwitchChannel,
                    user.CreatedAt,
                    user.IsOnline
                }
            });
        }

        /// <summary>
        /// Get user's friends list
        /// </summary>
        [HttpGet("{id}/friends")]
        public async Task<IActionResult> GetFriends(string id)
        {
            var user = await _userRepository.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            var friends = new List<User>();
            if (user.FriendIds != null && user.FriendIds.Any())
            {
                friends = await _userRepository.GetUsersAsync(user.FriendIds);
            }

            return Ok(new { 
                success = true, 
                friends = friends.Select(f => new {
                    f.Id,
                    f.Username,
                    f.AvatarUrl,
                    f.KarmaScore,
                    f.IsOnline
                })
            });
        }

        /// <summary>
        /// Add a friend
        /// </summary>
        [HttpPost("{id}/friends")]
        public async Task<IActionResult> AddFriend(string id, [FromBody] AddFriendRequest request)
        {
            var user = await _userRepository.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            await _userRepository.AddFriendAsync(id, request.FriendId);
            return Ok(new { success = true, message = "Friend added" });
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateProfileRequest request)
        {
            var user = await _userRepository.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            if (!string.IsNullOrEmpty(request.Username)) user.Username = request.Username;
            if (request.Bio != null) user.Bio = request.Bio;
            if (request.GameLibrary != null) user.GameLibrary = request.GameLibrary;
            if (request.DiscordUserId != null) user.DiscordUserId = request.DiscordUserId;
            if (request.SteamId != null) user.SteamId = request.SteamId;
            if (request.TwitchChannel != null) user.TwitchChannel = request.TwitchChannel;

            await _userRepository.UpdateUserAsync(user);
            return Ok(new { success = true, message = "Profile updated" });
        }

        /// <summary>
        /// Get all endorsements given to a user
        /// </summary>
        [HttpGet("{id}/endorsements")]
        public async Task<IActionResult> GetEndorsements(string id)
        {
            var endorsements = await _endorsementRepository.GetEndorsementsForUserAsync(id);
            return Ok(new { success = true, endorsements });
        }

        /// <summary>
        /// Give endorsement to a user
        /// </summary>
        [HttpPost("{id}/endorsements")]
        public async Task<IActionResult> GiveEndorsement(string id, [FromBody] GiveEndorsementRequest request)
        {
            var toUser = await _userRepository.GetUserAsync(id);
            if (toUser == null) return NotFound(new { success = false, error = "User not found" });

            var fromUser = await _userRepository.GetUserAsync(request.FromUserId);
            if (fromUser == null) return BadRequest(new { success = false, error = "FromUser not found" });

            var endorsement = new Endorsement
            {
                FromUserId = request.FromUserId,
                ToUserId = id,
                EndorsementType = request.Type,
                Comment = request.Comment ?? "",
                CreatedAt = DateTime.UtcNow
            };

            await _endorsementRepository.CreateEndorsementAsync(endorsement);
            await _karmaService.ProcessEndorsementAsync(id, fromUser.Username, request.Type, endorsement.Id);

            return Ok(new { success = true, endorsementId = endorsement.Id });
        }
    }

    // Request DTOs
    public class AddFriendRequest
    {
        public string FriendId { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string? Username { get; set; }
        public string? Bio { get; set; }
        public List<string>? GameLibrary { get; set; }
        public string? DiscordUserId { get; set; }
        public string? SteamId { get; set; }
        public string? TwitchChannel { get; set; }
    }

    public class GiveEndorsementRequest
    {
        public string FromUserId { get; set; } = string.Empty;
        public string Type { get; set; } = "Positive";
        public string? Comment { get; set; }
    }
}
