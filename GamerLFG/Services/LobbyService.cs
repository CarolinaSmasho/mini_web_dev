using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services.Interface;

namespace GamerLFG.Services
{
    public class LobbyService : ILobbyService
    {
        public async Task<List<Lobby>> GetAllLobbyAsync(){
            return [];
        }
        public async Task<Lobby> CreateLobbyAsync(Lobby newLobby){
            var nl = new Lobby() ;
            return nl;
        }
        public async Task DeleteLobbyAsync (string id){

        }
        public async Task UpdateLobbyAsync (Lobby lobby){

        }
        public async Task AddmemberAsync (Lobby current_lobby,User newUser){

        }
    }
}

