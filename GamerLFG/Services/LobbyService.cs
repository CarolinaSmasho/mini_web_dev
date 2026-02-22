using MongoDB.Driver;
using GamerLFG.Models;
using GamerLFG.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Services
{
    public class LobbyService : ILobbyService
    {
        private readonly IMongoCollection<Lobby> _lobbyCollection;

        public LobbyService(IMongoDatabase database)
        {
            _lobbyCollection = database.GetCollection<Lobby>("Lobbies");
        }

        public async Task<List<Lobby>> GetAllLobbiesAsync()
        {
            return await _lobbyCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Lobby?> GetLobbyByIdAsync(string id)
        {
            return await _lobbyCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Lobby> CreateLobbyAsync(CreateLobbyDTO dto)
        {
            var newLobby = new Lobby
            {
                Title = dto.Title,
                Game = dto.Game,
                Description = dto.Description,
                HostId = dto.HostId,
                MaxPlayers = dto.MaxPlayers,
                StartRecruiting = dto.StartRecruiting,
                IsRecruiting = true,
                // นำ Host เข้าเป็นสมาชิกคนแรกทันที
                HostId = new List<string> { dto.HostId },
                Members = new List<LobbyMember> 
                { 
                    new LobbyMember { UserId = dto.HostId, IsHost = true, AssignedRole = "Host" } 
                }
            };

            await _lobbyCollection.InsertOneAsync(newLobby);
            return newLobby;
        }

        public async Task<bool> ToggleRecruitmentAsync(string id)
        {
            var lobby = await GetLobbyByIdAsync(id);
            if (lobby == null) return false;

            var update = Builders<Lobby>.Update.Set(l => l.IsRecruiting, !lobby.IsRecruiting);
            var result = await _lobbyCollection.UpdateOneAsync(l => l.Id == id, update);
            
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ApplyToLobbyAsync(string lobbyId, string userId)
        {
            // ลอจิกจำลองการสมัครเข้าปาร์ตี้
            var update = Builders<Lobby>.Update.AddToSet(l => l.PlayerIds, userId);
            var result = await _lobbyCollection.UpdateOneAsync(l => l.Id == lobbyId, update);
            
            return result.ModifiedCount > 0;
        }
    }
}