using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace LibraryApi.Models;


[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReservationStatus
{
    [EnumMember(Value = "active")]
    Active,

    [EnumMember(Value = "on_course")]
    OnCourse,

    [EnumMember(Value = "cancelled")]
    Cancelled,

    [EnumMember(Value = "completed")]
    Completed
}

public class RoomReservationModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("room_id")]
    [BsonElement("room_id")]
    public string RoomId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("user_id")]
    [BsonElement("user_id")]
    public string UserId { get; set; } = null!;

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("start_time")]
    [BsonElement("start_time")]
    public DateTime StartTime { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("end_time")]
    [BsonElement("end_time")]
    public DateTime EndTime { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
    // active | cancelled | completed

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("created_at")]
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
