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
        // 🟢 1. เพิ่ม Collection สำหรับ Notification
        private readonly IMongoCollection<Notification> _notificationCollection;
        public FriendRequestService(IMongoDatabase database)
        {
            _requestCollection = database.GetCollection<FriendRequest>("FriendRequests");
            _userCollection = database.GetCollection<User>("Users");
            
            _notificationCollection = database.GetCollection<Notification>("Notifications");
        }

        public async Task<bool> SendRequestAsync(string senderId, string receiverId)
        {
            // ป้องกันการแอดตัวเองเป็นเพื่อน
            if (senderId == receiverId) return false;

            // เช็คก่อนว่าเคยส่งไปแล้วและยัง pending อยู่ไหม จะได้ไม่สร้างซ้ำ
            var existingRequest = await _requestCollection.Find(r => 
                ((r.UserSender == senderId && r.UserReceiver == receiverId) || 
                 (r.UserSender == receiverId && r.UserReceiver == senderId)) && 
                r.Status == "pending"
            ).FirstOrDefaultAsync();

            if (existingRequest != null) return false; // เคยส่งไปแล้ว ไม่ต้องทำอะไร

            // สร้างคำขอใหม่
            var newRequest = new FriendRequest
            {
                UserSender = senderId,
                UserReceiver = receiverId,
                Status = "pending",
                CreatedAt = DateTime.UtcNow
            };

            await _requestCollection.InsertOneAsync(newRequest);
            //noti part
            var sender = await _userCollection.Find(u => u.Id == senderId).FirstOrDefaultAsync();
            var senderName = sender != null ? sender.Username : "Someone";

            var notification = new Notification
            {
                Type = "friend_request",
                RelateObjectId = newRequest.Id,
                UserId = receiverId, // ส่งหาคนรับ
                Text = $"{senderName} ได้ส่งคำขอเป็นเพื่อนถึงคุณ",
                IsRead = false,
                Date = DateTime.UtcNow
            };
            await _notificationCollection.InsertOneAsync(notification);
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

            //เพิ่ม idfriend เข้าไปที่ user คนส่งคำขอ
            var updateSender = Builders<User>.Update.AddToSet(u => u.FriendIds, request.UserReceiver);
            await _userCollection.UpdateOneAsync(u => u.Id == request.UserSender, updateSender);
            //เพิ่ม idfriend เข้าไปที่ user คนรับ
            var updateReceiver = Builders<User>.Update.AddToSet(u => u.FriendIds, request.UserSender);
            await _userCollection.UpdateOneAsync(u => u.Id == request.UserReceiver, updateReceiver);
            //noti part
            var receiver = await _userCollection.Find(u => u.Id == request.UserReceiver).FirstOrDefaultAsync();
            var receiverName = receiver != null ? receiver.Username : "เพื่อนของคุณ";
            var notification = new Notification
            {
                Type = "friend_accept",
                RelateObjectId = request.UserReceiver,
                UserId = request.UserSender, // ส่งหาคนรับ
                Text = $"{receiverName} ได้ยอมรับคำขอเป็นเพื่อนของคุณแล้ว",
                IsRead = false,
                Date = DateTime.UtcNow
            };
            await _notificationCollection.InsertOneAsync(notification);
            return true;
        }

        public async Task<bool> RejectRequestAsync(string requestId)
        {
            // คำสั่ง DeleteOneAsync จะลบ Object แถวนั้นออกจาก Database ทิ้งไปเลย
            var result = await _requestCollection.DeleteOneAsync(r => r.Id == requestId);
            return result.DeletedCount > 0;
        }

        public async Task<List<FriendRequest>> GetPendingRequestsAsync(string userId)
        {
            // ดึงคำขอที่ "เราเป็นคนรับ (UserReceiver)" และสถานะยังเป็น "pending"
            var filter = Builders<FriendRequest>.Filter.Eq(r => r.UserReceiver, userId) & 
                         Builders<FriendRequest>.Filter.Eq(r => r.Status, "pending");
                         
            return await _requestCollection.Find(filter).ToListAsync();
        }

        public async Task<bool> HasPendingRequestAsync(string senderId, string receiverId)
        {
            var request = await _requestCollection.Find(r => 
                r.UserSender == senderId && 
                r.UserReceiver == receiverId && 
                r.Status == "pending"
            ).FirstOrDefaultAsync();
            
            return request != null;
        }

        public async Task<bool> CancelRequestAsync(string senderId, string receiverId)
        {
            var result = await _requestCollection.DeleteOneAsync(r => 
                r.UserSender == senderId && 
                r.UserReceiver == receiverId && 
                r.Status == "pending"
            );
            return result.DeletedCount > 0;
        }
    }
}