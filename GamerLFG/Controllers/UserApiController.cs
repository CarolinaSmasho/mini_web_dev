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
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly KarmaService _karmaService;
        private readonly NotificationService _notificationService;

        public UserApiController(
            IUserRepository userRepository, 
            IEndorsementRepository endorsementRepository, 
            IFriendRequestRepository friendRequestRepository,
            KarmaService karmaService,
            NotificationService notificationService)
        {
            _userRepository = userRepository;
            _endorsementRepository = endorsementRepository;
            _friendRequestRepository = friendRequestRepository;
            _karmaService = karmaService;
            _notificationService = notificationService;
        }



        /// <summary>
        /// Search users by username
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { success = false, error = "Query is required" });

            var users = await _userRepository.SearchUsersAsync(query);
            return Ok(new { 
                success = true, 
                users = users.Select(u => new {
                    u.Id,
                    u.Username,
                    u.AvatarUrl,
                    u.KarmaScore,
                    u.IsOnline
                })
            });
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
        /// Send a friend request
        /// </summary>
        [HttpPost("friend-request/send")]
        public async Task<IActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            var fromUser = await _userRepository.GetUserAsync(request.FromUserId);
            var toUser = await _userRepository.GetUserAsync(request.ToUserId);

            if (fromUser == null || toUser == null)
                return NotFound(new { success = false, error = "User not found" });

            if (fromUser.FriendIds.Contains(request.ToUserId))
                return BadRequest(new { success = false, error = "Already friends" });

            var existingRequest = await _friendRequestRepository.GetByUsersAsync(request.FromUserId, request.ToUserId);
            if (existingRequest != null && existingRequest.Status == "Pending")
                return BadRequest(new { success = false, error = "Request already pending" });

            var newRequest = new FriendRequest
            {
                FromUserId = request.FromUserId,
                ToUserId = request.ToUserId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _friendRequestRepository.CreateAsync(newRequest);
            
            // Notify recipient
            await _notificationService.NotifyUserAsync(request.ToUserId, "FriendRequest", $"You have a friend request from {fromUser.Username}", newRequest.Id);

            return Ok(new { success = true, message = "Friend request sent" });
        }

        /// <summary>
        /// Accept a friend request
        /// </summary>
        [HttpPost("friend-request/accept")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] RequestActionDto request)
        {
            var friendRequest = await _friendRequestRepository.GetByIdAsync(request.RequestId);
            if (friendRequest == null) return NotFound(new { success = false, error = "Request not found" });

            if (friendRequest.ToUserId != request.UserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            await _friendRequestRepository.UpdateStatusAsync(request.RequestId, "Accepted");

            // Add to both users' friend lists
            await _userRepository.AddFriendAsync(friendRequest.FromUserId, friendRequest.ToUserId);
            await _userRepository.AddFriendAsync(friendRequest.ToUserId, friendRequest.FromUserId);

            // Notify sender
            var user = await _userRepository.GetUserAsync(request.UserId);
            await _notificationService.NotifyUserAsync(friendRequest.FromUserId, "FriendRequest", $"{user?.Username ?? "Unknown"} accepted your friend request", request.RequestId);

            return Ok(new { success = true, message = "Friend request accepted" });
        }

        /// <summary>
        /// Reject a friend request
        /// </summary>
        [HttpPost("friend-request/reject")]
        public async Task<IActionResult> RejectFriendRequest([FromBody] RequestActionDto request)
        {
            var friendRequest = await _friendRequestRepository.GetByIdAsync(request.RequestId);
            if (friendRequest == null) return NotFound(new { success = false, error = "Request not found" });

            if (friendRequest.ToUserId != request.UserId)
                return Unauthorized(new { success = false, error = "Not authorized" });

            await _friendRequestRepository.UpdateStatusAsync(request.RequestId, "Rejected");
            return Ok(new { success = true, message = "Friend request rejected" });
        }

        /// <summary>
        /// Get pending friend requests
        /// </summary>
        [HttpGet("{userId}/friend-requests")]
        public async Task<IActionResult> GetPendingRequests(string userId)
        {
            var requests = await _friendRequestRepository.GetPendingByUserIdAsync(userId);
            
            // Enrich with FromUser details
            var fromUserIds = requests.Select(r => r.FromUserId).ToList();
            var fromUsers = await _userRepository.GetUsersAsync(fromUserIds);
            var userMap = fromUsers.ToDictionary(u => u.Id, u => u);

            var result = requests.Select(r => new {
                r.Id,
                r.FromUserId,
                r.CreatedAt,
                FromUserName = userMap.ContainsKey(r.FromUserId) ? userMap[r.FromUserId].Username : "Unknown",
                FromUserAvatar = userMap.ContainsKey(r.FromUserId) ? userMap[r.FromUserId].AvatarUrl : ""
            });

            return Ok(new { success = true, requests = result });
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

    public class FriendRequestDto
    {
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
    }

    public class RequestActionDto
    {
        public string RequestId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // User performing the action (ToUserId)
    }
}
