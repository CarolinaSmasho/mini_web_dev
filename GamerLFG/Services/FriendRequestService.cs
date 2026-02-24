using MongoDB.Driver;
using GamerLFG.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamerLFG.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IMongoCollection<FriendRequest> _requestCollection;
        private readonly IMongoCollection<User> _userCollection;

        public FriendRequestService(IMongoDatabase database)
        {
            // ดึงมาใช้งานทั้ง 2 ตารางเลย!
            _requestCollection = database.GetCollection<FriendRequest>("FriendRequests");
            _userCollection = database.GetCollection<User>("Users");
        }

        public async Task<bool> SendRequestAsync(string senderId, string receiverId)
        {
            // ป้องกันการแอดตัวเองเป็นเพื่อน
            if (senderId == receiverId) return false;

            // เช็คก่อนว่าเคยส่งไปแล้วและยัง pending อยู่ไหม จะได้ไม่สร้างซ้ำ
            var existingRequest = await _requestCollection.Find(r => 
                ((r.User1Id == senderId && r.User2Id == receiverId) || 
                 (r.User1Id == receiverId && r.User2Id == senderId)) && 
                r.Status == "pending"
            ).FirstOrDefaultAsync();

            if (existingRequest != null) return false; // เคยส่งไปแล้ว ไม่ต้องทำอะไร

            // สร้างคำขอใหม่
            var newRequest = new FriendRequest
            {
                User1Id = senderId,
                User2Id = receiverId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await _requestCollection.InsertOneAsync(newRequest);
            return true;
        }

        public async Task<bool> AcceptRequestAsync(string requestId)
        {
            // 1. หาคำขอนั้นให้เจอ
            var request = await _requestCollection.Find(r => r.Id == requestId).FirstOrDefaultAsync();
            if (request == null || request.Status != "pending") return false;

            // 2. เปลี่ยนสถานะเป็น accepted
            var updateRequest = Builders<FriendRequest>.Update.Set(r => r.Status, "accepted");
            await _requestCollection.UpdateOneAsync(r => r.Id == requestId, updateRequest);

            // 3. 🟢 อัปเดตตาราง User ฝั่งคนส่ง (User1Id) ให้มีเพื่อนเป็นคนรับ (User2Id)
            var updateSender = Builders<User>.Update.AddToSet(u => u.FriendIds, request.User2Id);
            await _userCollection.UpdateOneAsync(u => u.Id == request.User1Id, updateSender);

            // 4. 🟢 อัปเดตตาราง User ฝั่งคนรับ (User2Id) ให้มีเพื่อนเป็นคนส่ง (User1Id)
            var updateReceiver = Builders<User>.Update.AddToSet(u => u.FriendIds, request.User1Id);
            await _userCollection.UpdateOneAsync(u => u.Id == request.User2Id, updateReceiver);

            return true;
        }

        public async Task<List<FriendRequest>> GetPendingRequestsAsync(string userId)
        {
            // ดึงคำขอที่ "เราเป็นคนรับ (User2Id)" และสถานะยังเป็น "pending"
            var filter = Builders<FriendRequest>.Filter.Eq(r => r.User2Id, userId) & 
                         Builders<FriendRequest>.Filter.Eq(r => r.Status, "pending");
                         
            return await _requestCollection.Find(filter).ToListAsync();
        }
    }
}