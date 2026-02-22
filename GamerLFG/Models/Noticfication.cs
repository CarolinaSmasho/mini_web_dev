using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Type { get; set; } // e.g., 'friend_request', 'lobby_invite'
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string RelateObjectId { get; set; } // ID ของ FriendRequest หรือ Lobby ที่เกี่ยวข้อง

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } // ผู้รับแจ้งเตือน

    public string Text { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}