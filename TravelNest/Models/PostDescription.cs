using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
public class PostDescription
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    [BsonElement("userId")]
    public int UserId { get; set; }
    [BsonElement("postId")]
    public int PostId { get; set; }
    [BsonElement("description")]
    public string Description { get; set; }
    [BsonElement("status")]
    public string Status { get; set; } = "pending";
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
