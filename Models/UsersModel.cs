using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
// using System.Runtime.Serialization;

namespace LibraryApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserGender
{
    Male,
    Female,
    NonBinary,
    Other
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Customer,
    Admin,
}

public class UserModel
{
    [BsonId]                                  // MongoDB _id field
    [BsonRepresentation(BsonType.ObjectId)]   // Allows string IDs
    public string Id { get; set; } = null!;

    // [BsonElement("code")]
    // public string Code { get; set; } = null!;

    [BsonElement("username")]
    [BsonRequired]
    public string Username { get; set; } = null!;

    [BsonElement("first_name")]
    [JsonPropertyName("first_name")]
    [BsonRequired]
    public string FirstName { get; set; } = null!;

    [JsonPropertyName("last_name")]
    [BsonElement("last_name")]
    [BsonRequired]
    public string LastName { get; set; } = null!;

    [BsonElement("address")]
    [BsonRequired]
    public string Address { get; set; } = null!;

    [BsonElement("email")]
    [BsonRequired]
    public string Email { get; set; } = null!;

    [BsonElement("gender")]
    [BsonRepresentation(BsonType.String)]
    public UserGender Gender { get; set; }

    [BsonElement("password_hash")]
    [JsonPropertyName("password_hash")]
    [BsonRequired]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("date_of_birth")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    [JsonPropertyName("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public List<UserRole> Roles { get; set; } = new();
}

public class LoginModel
{
    public string username { get; set; } = String.Empty;
    public string password { get; set; } = String.Empty;
}
