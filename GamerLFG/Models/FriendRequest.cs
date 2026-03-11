using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class FriendRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserSender { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserReceiver { get; set; }

    public string Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}