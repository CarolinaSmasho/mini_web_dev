using GamerLFG.Models;
using GamerLFG.Repositories;

namespace GamerLFG.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> ValidateLoginAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return (user != null && user.Password == password) ? user : null;
        }

        public async Task<(bool success, string? error, string? userId)> RegisterAsync(
            string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || password != confirmPassword)
                return (false, "Please fill all fields correctly", null);

            if (await _userRepository.GetUserByEmailAsync(email) != null)
                return (false, "Email already registered", null);

            if (await _userRepository.GetUserByUsernameAsync(username) != null)
                return (false, "Username already taken", null);

            var newUser = new User
            {
                Username = username,
                Email = email,
                Password = password,
                KarmaScore = 0
            };

            await _userRepository.CreateUserAsync(newUser);
            return (true, null, newUser.Id);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user == null;
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            return user == null;
        }
    }
}
