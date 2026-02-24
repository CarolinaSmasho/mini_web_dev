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
            // ป้องกันกรณี User กดค้นหาช่องว่างๆ
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<User>();
            }

            // ใช้ Regex ในการค้นหา (ตัว "i" หมายถึง Case-Insensitive ไม่สนตัวพิมพ์เล็ก-ใหญ่)
            var filter = Builders<User>.Filter.Regex(u => u.Username, new BsonRegularExpression(keyword, "i"));
            
            // สั่งค้นหาและดึงข้อมูลออกมาเป็น List
            var searchResults = await _userCollection.Find(filter).ToListAsync();

            return searchResults;
        }


        public async Task<User> SearchUserAsync(string UserId)
        {
            var user = await _userCollection.Find(u => u.Id == UserId).FirstOrDefaultAsync();
            
            // // ถ้าไม่เจอตัวเรา หรือเราไม่มีเพื่อนเลย ให้ส่ง List ว่างๆ กลับไป
            // if (me == null || me.FriendIds == null || me.FriendIds.Count == 0)
            // {
            //     return new List<User>();
            // }

            return user;
        }

        // อย่าลืมไปเพิ่มการประกาศ method ใน IUserService.cs ด้วยนะ: 
        // Task<bool> RemoveFriendAsync(string currentUserId, string targetFriendId);

        public async Task<bool> RemoveFriendAsync(string currentUserId, string targetFriendId)
        {
            // ==========================================
            // ฝั่งที่ 1: เอาเพื่อนออกจากลิสต์ของเรา
            // ==========================================
            var filterMe = Builders<User>.Filter.Eq(u => u.Id, currentUserId);
            var updateMe = Builders<User>.Update.Pull(u => u.FriendIds, targetFriendId);
            
            // สั่งอัปเดตฝั่งเรา
            var resultMe = await _userCollection.UpdateOneAsync(filterMe, updateMe);


            // ==========================================
            // ฝั่งที่ 2: เอาเราออกจากลิสต์ของเพื่อนด้วย!
            // ==========================================
            var filterFriend = Builders<User>.Filter.Eq(u => u.Id, targetFriendId);
            var updateFriend = Builders<User>.Update.Pull(u => u.FriendIds, currentUserId);
            
            // สั่งอัปเดตฝั่งเพื่อน
            var resultFriend = await _userCollection.UpdateOneAsync(filterFriend, updateFriend);


            // คืนค่า true ถ้ามีการเปลี่ยนแปลงเกิดขึ้นอย่างน้อย 1 ฝั่ง 
            // (กันกรณีที่ฝั่งนึงอาจจะเผลอลบไปแล้ว แต่อีกฝั่งยังติดบัคค้างอยู่)
            return resultMe.ModifiedCount > 0 || resultFriend.ModifiedCount > 0;
        }

    }
}