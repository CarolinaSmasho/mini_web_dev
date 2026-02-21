using GamerLFG.Models;
using GamerLFG.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GamerLFG.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>("users");

        public IMongoCollection<Lobby> Lobbies =>
            _database.GetCollection<Lobby>("lobbies");
    }
}
