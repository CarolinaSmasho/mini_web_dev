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
            _lobbies = database.GetCollection<Lobby>("Lobby"); //ดึง collection lobby มาเก็บไว้เป็นตัวแปร
        }
        public async Task<LobbyListResponse> GetAllLobbyAsync(string? userId = null){ // ใช้function แบบ async
            
            var myLobbyList = await _lobbies.Find(mylobby => mylobby.HostId == userId).ToListAsync(); // ดึง lobby ของเรามาแบบ async
            var otherLobbyList = await _lobbies.Find(otherLobby => otherLobby.HostId != userId).Limit(10).ToListAsync();
            var myLobby = myLobbyList.Select( lob => new ShowLobbyDTO{
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
            var publicLobby = otherLobbyList.Select( lob => new ShowLobbyDTO{
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
            
            
        public async Task<List<ShowLobbyDTO>> GetNextLobbiesAsync(string? lastId,string? userId, int pageSize = 10)
            {
                // ใช้ .Ne() แทน .Eq()
                var filter = Builders<Lobby>.Filter.Ne(l => l.HostId, userId);

                
                // กรองเอาเฉพาะตัวที่ ID "หลังจาก" ตัวสุดท้ายที่หน้าจอแสดงอยู่
                filter = Builders<Lobby>.Filter.Gt(l => l.Id, lastId);
                var nextLobby = await _lobbies.Find(filter)
                                .SortBy(l => l.Id) // ต้องเรียงลำดับเสมอเพื่อให้ข้อมูลต่อเนื่องกัน
                                .Limit(pageSize)
                                .ToListAsync();
                
                

                return nextLobby.Select( lob => new ShowLobbyDTO{
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

