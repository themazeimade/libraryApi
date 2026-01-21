using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace LibraryApi.Models;

public class BookModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? id { get; set; }


    public string title { get; set; } = null!;
    public string author { get; set; } = null!;
    public string publisher { get; set; } = null!;
    public string isbn { get; set; } = null!;

    public int pages { get; set; }
    public int edition { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [BsonElement("date_published")]
    [JsonPropertyName("date_published")]
    public DateTime datePublished { get; set; }

    [BsonIgnoreIfNull]
    public String? description { get; set; } = null!;
}

