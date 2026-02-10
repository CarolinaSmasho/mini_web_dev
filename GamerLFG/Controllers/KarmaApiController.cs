using GamerLFG.Models;
using GamerLFG.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KarmaApiController : ControllerBase
    {
        private readonly KarmaService _karmaService;

        public KarmaApiController(KarmaService karmaService)
        {
            _karmaService = karmaService;
        }

        /// <summary>
        /// Get karma history for a user (public access)
        /// </summary>
        [HttpGet("{userId}/history")]
        public async Task<IActionResult> GetHistory(string userId, [FromQuery] int limit = 50)
        {
            var history = await _karmaService.GetKarmaHistoryAsync(userId, limit);
            return Ok(history);
        }

        /// <summary>
        /// Award karma to a user (for testing)
        /// </summary>
        [HttpPost("{userId}/award")]
        public async Task<IActionResult> Award(string userId, [FromBody] AwardKarmaRequest request)
        {
            await _karmaService.AwardKarmaAsync(
                userId,
                request.ActionType,
                request.Points,
                request.ReferenceType,
                request.ReferenceId,
                request.Description
            );

            return Ok(new { message = $"Awarded {request.Points} karma to user {userId}", actionType = request.ActionType });
        }

        /// <summary>
        /// Recalculate karma for a user
        /// </summary>
        [HttpPost("{userId}/recalculate")]
        public async Task<IActionResult> Recalculate(string userId)
        {
            await _karmaService.RecalculateKarmaAsync(userId);
            return Ok(new { message = $"Recalculated karma for user {userId}" });
        }
    }

    public class AwardKarmaRequest
    {
        public string ActionType { get; set; } = string.Empty;
        public int Points { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceId { get; set; }
        public string? Description { get; set; }
    }
}
