using GamerLFG.Models;

namespace GamerLFG.Services
{
    public interface IAuthService
    {
        Task<User?> ValidateLoginAsync(string email, string password);
        Task<(bool success, string? error, string? userId)> RegisterAsync(string username, string email, string password, string confirmPassword);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<bool> IsUsernameAvailableAsync(string username);
    }
}
