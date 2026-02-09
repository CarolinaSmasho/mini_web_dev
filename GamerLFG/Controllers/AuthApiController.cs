using Microsoft.AspNetCore.Mvc;
using GamerLFG.Models;
using GamerLFG.Repositories;

namespace GamerLFG.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public AuthApiController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { success = false, error = "Email and password are required" });

            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user != null && user.Password == request.Password)
            {
                return Ok(new { 
                    success = true, 
                    message = "Login successful",
                    user = new {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.KarmaScore
                    }
                });
            }

            return Unauthorized(new { success = false, error = "Invalid email or password" });
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Username) || 
                string.IsNullOrEmpty(request.Email) || 
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { success = false, error = "Username, email, and password are required" });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { success = false, error = "Passwords do not match" });
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { success = false, error = "Email already registered" });
            }

            var existingUsername = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (existingUsername != null)
            {
                return Conflict(new { success = false, error = "Username already taken" });
            }

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                KarmaScore = 0 // Start with 0 karma
            };

            await _userRepository.CreateUserAsync(newUser);

            return Created($"/api/user/{newUser.Id}", new { 
                success = true, 
                message = "Account created successfully",
                userId = newUser.Id
            });
        }

        /// <summary>
        /// Check if email is available
        /// </summary>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return Ok(new { available = user == null });
        }

        /// <summary>
        /// Check if username is available
        /// </summary>
        [HttpGet("check-username")]
        public async Task<IActionResult> CheckUsername([FromQuery] string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            return Ok(new { available = user == null });
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
