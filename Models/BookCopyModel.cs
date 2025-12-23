using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryApi.Models;

public class BookCopyModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? BookId { get; set; }

    public int Code { get; set; }
    public bool Available { get; set; } = true;
    public bool Dismissed { get; set; } = false;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DateAcquired { get; set; }
}

