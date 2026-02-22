using MongoDB.Driver;
using GamerLFG.Models;
using Microsoft.Extensions.Options;

namespace GamerLFG.service
{
    public class MongoDBservice
    {
        private readonly IMongoDatabase database;

        public MongoDBservice(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => database.GetCollection<User>("Users");
        public IMongoCollection<Lobby> Lobbies => database.GetCollection<Lobby>("Lobbies");
        public IMongoCollection<Application> Applications => database.GetCollection<Application>("Applications");
        public IMongoCollection<Product> Products => database.GetCollection<Product>("Products");
    }
}