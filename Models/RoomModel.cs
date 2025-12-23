using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace LibraryApi.Models;

public class RoomModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? Id { get; set; }

    [BsonElement("code")]
    public string Code { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("capacity")]
    public int Capacity { get; set; }

    [BsonElement("location")]
    public string Location { get; set; } = null!;

    [BsonElement("features")]
    public string[] Features { get; set; } = null!;

    //this is if the room is on remodeling, or anything that impedes intended users from using it.
    [BsonElement("is_active")]
    [JsonPropertyName("is_active")]
    public string IsActive { get; set; } = null!;
}
