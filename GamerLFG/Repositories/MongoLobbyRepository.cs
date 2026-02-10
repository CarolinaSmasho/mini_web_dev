using GamerLFG.Data;
using GamerLFG.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public class MongoLobbyRepository : ILobbyRepository
    {
        private readonly MongoDbContext _context;

        public MongoLobbyRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lobby>> GetLobbiesAsync()
        {
            return await _context.Lobbies.Find(_ => true).ToListAsync();
        }

        public async Task<Lobby> GetLobbyAsync(string id)
        {
            return await _context.Lobbies.Find(l => l.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateLobbyAsync(Lobby lobby)
        {
            await _context.Lobbies.InsertOneAsync(lobby);
        }

        public async Task UpdateLobbyAsync(Lobby lobby)
        {
            await _context.Lobbies.ReplaceOneAsync(l => l.Id == lobby.Id, lobby);
        }

        public async Task DeleteLobbyAsync(string id)
        {
            await _context.Lobbies.DeleteOneAsync(l => l.Id == id);
        }

        public async Task AddMemberAsync(string lobbyId, Member member)
        {
            var update = Builders<Lobby>.Update.Push(l => l.Members, member);
            await _context.Lobbies.UpdateOneAsync(l => l.Id == lobbyId, update);
        }

        public async Task RemoveMemberAsync(string lobbyId, string userId)
        {
             var update = Builders<Lobby>.Update.PullFilter(l => l.Members, m => m.UserId == userId);
             await _context.Lobbies.UpdateOneAsync(l => l.Id == lobbyId, update);
        }
    }
}
