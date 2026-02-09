using GamerLFG.Data;
using GamerLFG.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly MongoDbContext _context;

        public MongoUserRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserAsync(string id)
        {
            return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            // Case insensitive search might be needed, but sticking to basic for now
            return await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task AddFriendAsync(string userId, string friendId)
        {
            var update = Builders<User>.Update.AddToSet(u => u.FriendIds, friendId);
            await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<List<User>> GetUsersAsync(List<string> ids)
        {
            var filter = Builders<User>.Filter.In(u => u.Id, ids);
            return await _context.Users.Find(filter).ToListAsync();
        }
    }
}
