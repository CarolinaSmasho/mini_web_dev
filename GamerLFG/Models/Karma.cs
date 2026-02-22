using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class KarmaHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string TargetUserId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string FromUserId { get; set; }

    public double Score { get; set; }
    public string Comment { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
}