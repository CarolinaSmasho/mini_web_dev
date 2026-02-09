using GamerLFG.Data;
using GamerLFG.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public class MongoApplicationRepository : IApplicationRepository
    {
        private readonly MongoDbContext _context;

        public MongoApplicationRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Application> GetApplicationAsync(string id)
        {
            return await _context.Applications.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Application>> GetApplicationsByLobbyIdAsync(string lobbyId)
        {
            return await _context.Applications.Find(a => a.LobbyId == lobbyId).ToListAsync();
        }

        public async Task<List<Application>> GetApplicationsByUserIdAsync(string userId)
        {
            return await _context.Applications.Find(a => a.UserId == userId).ToListAsync();
        }

        public async Task CreateApplicationAsync(Application application)
        {
            await _context.Applications.InsertOneAsync(application);
        }

        public async Task UpdateApplicationStatusAsync(string id, string status)
        {
            var update = Builders<Application>.Update.Set(a => a.Status, status);
            await _context.Applications.UpdateOneAsync(a => a.Id == id, update);
        }

        public async Task DeleteApplicationAsync(string id)
        {
            await _context.Applications.DeleteOneAsync(a => a.Id == id);
        }

        public async Task DeleteApplicationsByLobbyIdAsync(string lobbyId)
        {
            await _context.Applications.DeleteManyAsync(a => a.LobbyId == lobbyId);
        }
    }
}
