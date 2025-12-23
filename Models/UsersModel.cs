using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace LibraryApi.Models;


public class UserModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string? Id { get; set; }

    [BsonElement("code")]
    public string Code { get; set; } = null!;

    [BsonElement("code")]
    public string Username { get; set; } = null!;

    [BsonElement("first_name")]
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = null!;

    [JsonPropertyName("last_name")]
    [BsonElement("last_name")]
    public string LastName { get; set; } = null!;

    [BsonElement("address")]
    public string Address { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("password_hash")]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("role")]
    public string Role { get; set; } = null!;
}

public class LoginModel
{
    public string username { get; set; } = String.Empty;
    public string password { get; set; } = String.Empty;
}
