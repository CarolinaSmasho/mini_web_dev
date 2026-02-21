using GamerLFG.Models;
using MongoDB.Driver;

namespace GamerLFG.Services
{
    public class HomeService
    {
        private readonly MongoDBService _mongoDBService;

        public HomeService(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        public async Task<List<Lobby>> GetAllLobbys()
        {
            return await _mongoDBService.Lobbies.Find(_ => true).ToListAsync();
        }
    }
}
