using GamerLFG.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using GamerLFG.Data;

namespace GamerLFG.Repositories
{
    public class MongoNotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;

        public MongoNotificationRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _notifications = database.GetCollection<Notification>("Notifications");
            
            // Create specific index for TTL (30 days) if not exists
            var indexKeysDefinition = Builders<Notification>.IndexKeys.Ascending(n => n.CreatedAt);
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
            var indexModel = new CreateIndexModel<Notification>(indexKeysDefinition, indexOptions);
            _notifications.Indexes.CreateOne(indexModel);
        }

        public async Task CreateAsync(Notification notification)
        {
            await _notifications.InsertOneAsync(notification);
        }

        public async Task<List<Notification>> GetByUserIdAsync(string userId, int limit = 20)
        {
            return await _notifications.Find(n => n.UserId == userId)
                                       .SortByDescending(n => n.CreatedAt)
                                       .Limit(limit)
                                       .ToListAsync();
        }

        public async Task<long> GetUnreadCountAsync(string userId)
        {
            return await _notifications.CountDocumentsAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(string id)
        {
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _notifications.UpdateOneAsync(n => n.Id == id, update);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _notifications.UpdateManyAsync(n => n.UserId == userId && !n.IsRead, update);
        }

        public async Task DeleteAsync(string id)
        {
            await _notifications.DeleteOneAsync(n => n.Id == id);
        }
    }
}
