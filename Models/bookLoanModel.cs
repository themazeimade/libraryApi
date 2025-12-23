using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using LibraryApi.Utilities;

namespace LibraryApi.Models;

public class BookLoanModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("user_id")]
    [BsonElement("user_id")]
    public string? UserId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("book_copy_id")]
    [BsonElement("book_copy_id")]
    public string? BookCopyId { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("borrowed_at")]
    [BsonElement("borrowed_at")]
    public DateTime BorrowedAt { get; set; } = DateTime.UtcNow;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("due_date")]
    [BsonElement("due_date")]
    public DateTime DueDate { get; set; } = DateUtils.AddBusinessDays(DateTime.UtcNow, 10);

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("returned_at")]
    [BsonElement("returned_at")]
    public DateTime? ReturnedAt { get; set; } // nullable if not returned

    public string Status { get; set; } = "active"; // active | returned | overdue

    public double Fine { get; set; } = 0.0;
}

