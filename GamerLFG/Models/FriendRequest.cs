using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class FriendRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string User1Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string User2Id { get; set; }

    public string Status { get; set; } // 'pending', 'accepted'
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}