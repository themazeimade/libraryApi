using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace LibraryApi.Models;


public class EmployeeModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? Id { get; set; }

    [BsonElement("code")]
    public string Code { get; set; } = null!;

    [BsonElement("first_name")]
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = null!;

    [BsonElement("last_name")]
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = null!;

    [BsonElement("address")]
    public string Address { get; set; } = null!;
}

