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
        }
        public async Task<LobbyListResponse> GetAllLobbyAsync(string userId){
            
            var lobbyList = await _lobbies.Find(_ => true).ToListAsync();
            var separatedLobbies = lobbyList.ToLookup(lob => lob.HostId == userId);
            var myLobby = separatedLobbies[true].Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = lob.HostName,
                Picture = lob.Picture,
                Moods = lob.Moods,
                Currentplayers = lob.Members.Count,
                MaxPlayers = lob.MaxPlayers
                
            }).ToList();
            var publicLobby = separatedLobbies[false].Select( lob => new ShowLobbyDTO{
                Id = lob.Id,
                Title  = lob.Title,
                Game = lob.Game,
                Description = lob.Description,
                HostName  = lob.HostName,
                Picture = lob.Picture,
                Moods = lob.Moods,
                Currentplayers = lob.Members.Count,
                MaxPlayers = lob.MaxPlayers
                
            }).ToList();

            return new LobbyListResponse
            {
                MyLobbies = myLobby,
                OtherLobbies = publicLobby
            };
        

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

