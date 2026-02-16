namespace GamerLFG.Models
{
    public class Lobby
    {
        public string lobbyId {get;set;}
        public string hostId {get;set;}
        public string title {get;set;}
        public string game {get;set;}
        public string[] id_pending {get;set;}
        public string[] id_confirm {get;set;}
        public string description {get;set;}
        public int currentPlayers {get;set;}
        public int maxPlayers {get;set;}
        public string[] moods {get;set;}
        public string[] roles {get;set;}
        public bool isRecruiting {get;set;}
        public DateTime scheduleTime {get;set;}
        public DateTime createdAt {get;set;}

        public string gamePic {get;set;}
    }
}  