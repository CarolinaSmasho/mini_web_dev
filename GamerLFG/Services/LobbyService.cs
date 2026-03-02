using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services.Interface;
using GamerLFG.Services.Interface.DTOs;
using MongoDB.Driver;
namespace GamerLFG.Services
{
    public class LobbyService : ILobbyService
    {   
         private readonly MongoDBservice _database;
        public LobbyService(MongoDBservice database)
        {
            _database = database;
            
        }
     
        // _database.Users;
        public async Task<LobbyListResponse> GetAllLobbyAsync(string? userId = null){ // ใช้function แบบ async
            
            var myLobbyList = await _database.Lobbies.Find(mylobby => mylobby.HostId == userId).ToListAsync(); // ดึง lobby ของเรามาแบบ async
            var otherLobbyList = await _database.Lobbies.Find(otherLobby => otherLobby.HostId != userId).SortBy(l => l.Id).Limit(10).ToListAsync();
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
                var notYours = Builders<Lobby>.Filter.Ne(l => l.HostId, userId);

                
                // กรองเอาเฉพาะตัวที่ ID "หลังจาก" ตัวสุดท้ายที่หน้าจอแสดงอยู่
                var filter = Builders<Lobby>.Filter.Gt(l => l.Id, lastId);
                var nextLobby = await _database.Lobbies.Find(filter & notYours)
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

        public async Task<(bool success, string message)> CreateLobbyAsync(CreateLobbyDTO newLobby) 
        {
            try {
                var lobby = newLobby.ToEntity();
                
                // Debug ดูว่า Entity พร้อมใช้งานไหม
                Console.WriteLine($"Inserting Lobby: {System.Text.Json.JsonSerializer.Serialize(lobby)}");

                await _database.Lobbies.InsertOneAsync(lobby);
                
                // ถ้ามาถึงตรงนี้ได้ แสดงว่าคำสั่งส่งออกไปสำเร็จ
                return (true, "OK");
            }
            catch (MongoException ex) {
                // ดัก Error เฉพาะจาก MongoDB
                Console.WriteLine($"MongoDB Error: {ex.Message}");
                return (false, ex.Message);
            }
            catch (Exception ex) {
                // ดัก Error อื่นๆ เช่น Mapping พัง
                Console.WriteLine($"General Error: {ex.Message}");
                return (false, ex.Message);
            }
        }
                // public async Task<(bool success,string message)> CreateLobbyAsync(CreateLobbyDTO newLobby){
        //     var lobby = newLobby.ToEntity();
        //     Console.Write(newLobby);
        //     // var hostObj = await _users.Find(u => u.Id == lobby.HostId).FirstOrDefaultAsync();
        //     // if (hostObj == null)
        //     //     {
        //     //     return (false,"Host not found");
        //     //     }
        //     // string hostName = hostObj.Username;
        //     await _database.Lobbies.InsertOneAsync(lobby);
        //     return (true,"OK");
        // }
        public async Task DeleteLobbyAsync (string id){

        }
        public async Task UpdateLobbyAsync (Lobby lobby){

        }
        public async Task AddmemberAsync (Lobby current_lobby,User newUser){

        }
    }
}

