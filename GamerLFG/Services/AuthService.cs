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

        // 1. ตรวจสอบว่ามี User หรือ Email ซ้ำไหม
        public async Task<bool> IsUserExists(string username, string email)
        {
            return await _users.Find(u => u.Username == username || u.Email == email).AnyAsync();
        }

        
        public async Task RegisterAsync(User user, string plainPassword)
        {
            // Hash 
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            
            // ตั้งค่าเริ่มต้นอื่นๆ
            user.CreatedAt = DateTime.UtcNow;
            user.KarmaScore = 0;
            
            await _users.InsertOneAsync(user);
        }

        // Find user by email   
        public async Task<User> GetByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        // Find user by username
        public async Task<User> GetByUsernameAsync(string username) =>
            await _users.Find(u => u.Username == username).FirstOrDefaultAsync();

        // Insert new user
        
        public async Task<User?> VerifyLoginAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if(user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }
    }
}