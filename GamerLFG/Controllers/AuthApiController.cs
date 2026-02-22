using Microsoft.AspNetCore.Mvc;
using GamerLFG.Services;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthApiController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { success = false, error = "Email and password are required" });

            var user = await _authService.ValidateLoginAsync(request.Email, request.Password);
            if (user == null)
                return Unauthorized(new { success = false, error = "Invalid email or password" });

            return Ok(new
            {
                success = true,
                message = "Login successful",
                user = new { user.Id, user.Username, user.Email, user.KarmaScore }
            });
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (success, error, userId) = await _authService.RegisterAsync(
                request.Username, request.Email, request.Password, request.ConfirmPassword);

            if (!success)
            {
                if (error == "Email already registered" || error == "Username already taken")
                    return Conflict(new { success = false, error });
                return BadRequest(new { success = false, error });
            }

            return Created($"/api/user/{userId}", new
            {
                success = true,
                message = "Account created successfully",
                userId
            });
        }

        /// <summary>
        /// Check if email is available
        /// </summary>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var available = await _authService.IsEmailAvailableAsync(email);
            return Ok(new { available });
        }

        /// <summary>
        /// Check if username is available
        /// </summary>
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsername([FromQuery] string username)
        {
            var available = await _authService.IsUsernameAvailableAsync(username);
            return Ok(new { available });
        }
    }

    // Request DTOs
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
