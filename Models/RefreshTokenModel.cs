using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryApi.Models;

public class RefreshTokenModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string Id { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? UserId { get; set; }

    [BsonElement("token")]
    [BsonRepresentation(BsonType.String)]   // Allows string IDs
    public string Token { get; set; } = null!;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonElement("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    // [BsonRepresentation(BsonType.String)]   // Allows string IDs
    [BsonElement("replaced_by")]
    public string? ReplacedBy { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonElement("replaced_at")]
    public DateTime? ReplacedAt { get; set; }
}
