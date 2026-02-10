using GamerLFG.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamerLFG.Data;

namespace GamerLFG.Repositories
{
    public class MongoFriendRequestRepository : IFriendRequestRepository
    {
        private readonly IMongoCollection<FriendRequest> _requests;

        public MongoFriendRequestRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _requests = database.GetCollection<FriendRequest>("FriendRequests");
        }

        public async Task CreateAsync(FriendRequest request)
        {
            await _requests.InsertOneAsync(request);
        }

        public async Task<FriendRequest> GetByIdAsync(string id)
        {
            return await _requests.Find(r => r.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<FriendRequest>> GetPendingByUserIdAsync(string userId)
        {
            return await _requests.Find(r => r.ToUserId == userId && r.Status == "Pending").ToListAsync();
        }

        public async Task<FriendRequest> GetByUsersAsync(string fromUserId, string toUserId)
        {
            return await _requests.Find(r => r.FromUserId == fromUserId && r.ToUserId == toUserId).FirstOrDefaultAsync();
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            var update = Builders<FriendRequest>.Update.Set(r => r.Status, status);
            await _requests.UpdateOneAsync(r => r.Id == id, update);
        }
        
        public async Task DeleteAsync(string id)
        {
            await _requests.DeleteOneAsync(r => r.Id == id);
        }
    }
}
