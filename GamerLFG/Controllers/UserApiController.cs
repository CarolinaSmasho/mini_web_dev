using Microsoft.AspNetCore.Mvc;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserApiController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Search users by username
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { success = false, error = "Query is required" });

            var users = await _userService.SearchUsersAsync(query);
            return Ok(new
            {
                success = true,
                users = users.Select(u => new { u.Id, u.Username, u.AvatarUrl, u.KarmaScore, u.IsOnline })
            });
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            return Ok(new
            {
                success = true,
                user = new
                {
                    user.Id, user.Username, user.Email, user.Bio, user.AvatarUrl,
                    user.KarmaScore, user.VibeTags, user.GameLibrary, user.DiscordUserId,
                    user.SteamId, user.TwitchChannel, user.CreatedAt, user.IsOnline
                }
            });
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            return Ok(new
            {
                success = true,
                user = new
                {
                    user.Id, user.Username, user.Bio, user.AvatarUrl,
                    user.KarmaScore, user.VibeTags, user.GameLibrary, user.DiscordUserId,
                    user.SteamId, user.TwitchChannel, user.CreatedAt, user.IsOnline
                }
            });
        }

        /// <summary>
        /// Get user's friends list
        /// </summary>
        [HttpGet("{id}/friends")]
        public async Task<IActionResult> GetFriends(string id)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            var friends = await _userService.GetFriendsAsync(id);
            return Ok(new
            {
                success = true,
                friends = friends.Select(f => new { f.Id, f.Username, f.AvatarUrl, f.KarmaScore, f.IsOnline })
            });
        }

        /// <summary>
        /// Add a friend
        /// </summary>
        [HttpPost("{id}/friends")]
        public async Task<IActionResult> AddFriend(string id, [FromBody] AddFriendRequest request)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            await _userService.AddFriendAsync(id, request.FriendId);
            return Ok(new { success = true, message = "Friend added" });
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateProfileRequest request)
        {
            var user = await _userService.GetUserAsync(id);
            if (user == null) return NotFound(new { success = false, error = "User not found" });

            await _userService.UpdateProfileAsync(id, request.Username, request.Bio,
                request.GameLibrary != null ? string.Join(",", request.GameLibrary) : null,
                request.DiscordUserId, request.SteamId, request.TwitchChannel);

            return Ok(new { success = true, message = "Profile updated" });
        }

        /// <summary>
        /// Send a friend request
        /// </summary>
        [HttpPost("friend-request/send")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            var (success, error) = await _userService.SendFriendRequestAsync(request.FromUserId, request.ToUserId);
            if (!success)
            {
                if (error == "User not found") return NotFound(new { success = false, error });
                return BadRequest(new { success = false, error });
            }
            return Ok(new { success = true, message = "Friend request sent" });
        }

        /// <summary>
        /// Accept a friend request
        /// </summary>
        [HttpPost("friend-request/accept")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] RequestActionDto request)
        {
            var (success, error) = await _userService.AcceptFriendRequestAsync(request.RequestId, request.UserId);
            if (!success)
            {
                if (error == "Not authorized") return Unauthorized(new { success = false, error });
                return NotFound(new { success = false, error });
            }
            return Ok(new { success = true, message = "Friend request accepted" });
        }

        /// <summary>
        /// Reject a friend request
        /// </summary>
        [HttpPost("friend-request/reject")]
        public async Task<IActionResult> RejectFriendRequest([FromBody] RequestActionDto request)
        {
            var (success, error) = await _userService.RejectFriendRequestAsync(request.RequestId, request.UserId);
            if (!success)
            {
                if (error == "Not authorized") return Unauthorized(new { success = false, error });
                return NotFound(new { success = false, error });
            }
            return Ok(new { success = true, message = "Friend request rejected" });
        }

        /// <summary>
        /// Get pending friend requests
        /// </summary>
        [HttpGet("{userId}/friend-requests")]
        public async Task<IActionResult> GetPendingRequests(string userId)
        {
            var requests = await _userService.GetPendingRequestsAsync(userId);
            var result = requests.Select(r => new
            {
                r.Id,
                r.FromUserId,
                r.CreatedAt,
                FromUserName = r.FromUserName,
                FromUserAvatar = r.FromUserAvatar
            });
            return Ok(new { success = true, requests = result });
        }

        /// <summary>
        /// Get all endorsements given to a user
        /// </summary>
        [HttpGet("{id}/endorsements")]
        public async Task<IActionResult> GetEndorsements(string id)
        {
            var endorsements = await _userService.GetEndorsementsAsync(id);
            return Ok(new { success = true, endorsements });
        }

        /// <summary>
        /// Give endorsement to a user
        /// </summary>
        [HttpPost("{id}/endorsements")]
        public async Task<IActionResult> GiveEndorsement(string id, [FromBody] GiveEndorsementRequest request)
        {
            var (success, error, endorsementId) = await _userService.GiveEndorsementAsync(id, request.FromUserId, request.Type, request.Comment);
            if (!success)
            {
                if (error == "User not found" || error == "FromUser not found")
                    return NotFound(new { success = false, error });
                return BadRequest(new { success = false, error });
            }
            return Ok(new { success = true, endorsementId });
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

    public class FriendRequestDto
    {
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
    }

    public class RequestActionDto
    {
        public string RequestId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
