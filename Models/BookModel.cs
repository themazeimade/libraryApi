using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibraryApi.Models;

public class BookModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? Id { get; set; }

    public string Title { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Publisher { get; set; } = null!;
    public string Isbn { get; set; } = null!;

    public int Pages { get; set; }
    public int Edition { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DatePublished { get; set; }

    [BsonIgnoreIfNull]
    public String? Description { get; set; } = null!;
}

