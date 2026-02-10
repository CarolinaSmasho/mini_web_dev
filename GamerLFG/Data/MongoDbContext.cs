using GamerLFG.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GamerLFG.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Lobby> Lobbies => _database.GetCollection<Lobby>("Lobbies");
        public IMongoCollection<Application> Applications => _database.GetCollection<Application>("Applications");
        public IMongoCollection<Vote> Votes => _database.GetCollection<Vote>("Votes");
        public IMongoCollection<Endorsement> Endorsements => _database.GetCollection<Endorsement>("Endorsements");
        public IMongoCollection<KarmaHistory> KarmaHistories => _database.GetCollection<KarmaHistory>("KarmaHistories");
    }
}
