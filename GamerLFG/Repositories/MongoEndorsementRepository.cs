using GamerLFG.Data;
using GamerLFG.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Repositories
{
    public class MongoEndorsementRepository : IEndorsementRepository
    {
        private readonly MongoDbContext _context;

        public MongoEndorsementRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<List<Endorsement>> GetEndorsementsForUserAsync(string userId)
        {
            return await _context.Endorsements.Find(e => e.ToUserId == userId).ToListAsync();
        }

        public async Task CreateEndorsementAsync(Endorsement endorsement)
        {
            await _context.Endorsements.InsertOneAsync(endorsement);
        }
    }
}
