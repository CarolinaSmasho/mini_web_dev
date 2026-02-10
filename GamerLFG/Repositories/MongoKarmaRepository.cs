using GamerLFG.Data;
using GamerLFG.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public class MongoKarmaRepository : IKarmaRepository
    {
        private readonly MongoDbContext _context;

        public MongoKarmaRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(KarmaHistory entry)
        {
            await _context.KarmaHistories.InsertOneAsync(entry);
        }

        public async Task<List<KarmaHistory>> GetByUserIdAsync(string userId, int limit = 50)
        {
            return await _context.KarmaHistories
                .Find(k => k.UserId == userId)
                .SortByDescending(k => k.CreatedAt)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<int> GetTotalPointsByUserIdAsync(string userId)
        {
            var entries = await _context.KarmaHistories
                .Find(k => k.UserId == userId)
                .ToListAsync();
            
            return entries.Sum(e => e.Points);
        }
    }
}
