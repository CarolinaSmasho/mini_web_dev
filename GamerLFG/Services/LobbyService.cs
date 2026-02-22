using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services.Interface;
using GamerLFG.Services.Interface.DTOs;
using MongoDB.Driver;
namespace GamerLFG.Services
{
    public class LobbyService : ILobbyService
    {   
        private readonly IMongoCollection<Lobby> _lobbies;
        private readonly IMongoCollection<User> _users;
        public LobbyService(IMongoDatabase database)
        {
            _lobbies = database.GetCollection<Lobby>("Lobby");
            _users = database.GetCollection<User>("User");
        }
        public async Task<List<ShowLobbyDTO>> GetAllLobbyAsync(){
            var host_name = _lobbies.Aggregate(){
                
            }
            var lobbyList = await _lobbies.Find(_ => true).ToListAsync();
            return lobbyList.Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = lob.HostId,
                Picture = lob.Picture,
                Moods = lob.Moods,
                Currentplayers = lob.Members.Count
            });   
            }
            
            
        }
        public async Task CreateLobbyAsync(CreateLobbyDTO newLobby){
            
            
        }
        public async Task DeleteLobbyAsync (string id){

        }
        public async Task UpdateLobbyAsync (Lobby lobby){

        }
        public async Task AddmemberAsync (Lobby current_lobby,User newUser){

        }
    }
}

