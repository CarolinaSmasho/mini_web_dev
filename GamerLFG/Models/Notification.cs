using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace GamerLFG.Models{
public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Type { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string RelateObjectId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }

    public string Text { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}
}