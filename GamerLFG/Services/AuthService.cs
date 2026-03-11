using MongoDB.Driver;
using GamerLFG.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace GamerLFG.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _users;

        public AuthService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _users = database.GetCollection<User>("Users");
        }

        public async Task<bool> IsUserExists(string username, string email)
        {
            return await _users.Find(u => u.Username == username || u.Email == email).AnyAsync();
        }

        
        public async Task RegisterAsync(User user, string plainPassword)
        {

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            

            user.CreatedAt = DateTime.UtcNow;
            user.KarmaScore = 3.0; // Bayesian neutral default (globalAvg) — ยังไม่ถูกโหวต
            
            await _users.InsertOneAsync(user);
        }

        public async Task<User> GetByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<User> GetByUsernameAsync(string username) =>
            await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

        
        public async Task<User?> VerifyLoginAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            try
            {
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return null;
            }
            catch (BCrypt.Net.SaltParseException)
            {

                return null;
            }

            return user;
        }
    }
}