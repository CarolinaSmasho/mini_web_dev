using MongoDB.Driver;
using MongoDB.Bson;
using GamerLFG.Models;
using GamerLFG.service;

namespace GamerLFG.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IMongoDatabase database)
        {
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetMyFriendsAsync(string myUserId)
        {
            var me = await _userCollection.Find(u => u.Id == myUserId).FirstOrDefaultAsync();

            var filter = Builders<User>.Filter.In(u => u.Id, me.FriendIds);
            var myFriends = await _userCollection.Find(filter).ToListAsync();

            return myFriends;
        }

        public async Task<List<User>> SearchFriendAsync(string keyword)
        {

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<User>();
            }

            var filter = Builders<User>.Filter.Regex(u => u.Username, new BsonRegularExpression(keyword, "i"));
            
            var searchResults = await _userCollection.Find(filter).ToListAsync();

            return searchResults;
        }

        public async Task<User> SearchUserAsync(string UserId)
        {
            var user = await _userCollection.Find(u => u.Id == UserId).FirstOrDefaultAsync();
            
            return user;
        }

        public async Task<bool> RemoveFriendAsync(string currentUserId, string targetFriendId)
        {

            var filterMe = Builders<User>.Filter.Eq(u => u.Id, currentUserId);
            var updateMe = Builders<User>.Update.Pull(u => u.FriendIds, targetFriendId);
            
            var resultMe = await _userCollection.UpdateOneAsync(filterMe, updateMe);

            var filterFriend = Builders<User>.Filter.Eq(u => u.Id, targetFriendId);
            var updateFriend = Builders<User>.Update.Pull(u => u.FriendIds, currentUserId);
            
            var resultFriend = await _userCollection.UpdateOneAsync(filterFriend, updateFriend);

            return resultMe.ModifiedCount > 0 || resultFriend.ModifiedCount > 0;
        }

    }
}