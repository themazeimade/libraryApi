using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.AspNetCore.Identity;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly MongoDbService _mongo;

    public UserController(MongoDbService mongo)
    {
        _mongo = mongo;
    }
    // Example GET endpoint (empty for now)
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var user = await _mongo.Users.Find(_ => true).ToListAsync();
        return Ok(user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid user id");

        var user = await _mongo.Users
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // Example POST endpoint (empty for now)
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserModel user)
    {
        if (user.Roles.Contains(UserRole.Admin)) return BadRequest("You can't give yourself admin role");
        user.Roles = [];
        user.Roles.Add(UserRole.Customer);
        var hasher = new PasswordHasher<UserModel>();
        user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);
        await _mongo.Users.InsertOneAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserModel user)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid user id");

        user.Id = id;

        var res = await _mongo.Users.ReplaceOneAsync(s => s.Id == user.Id, user);

        if (res.MatchedCount == 0) return NotFound();

        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {

        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid user id");

        var res = await _mongo.Users.DeleteOneAsync(s => s.Id == id);

        if (res.DeletedCount == 0) return NotFound();

        return Ok(res);
    }

    // You can add other HTTP verbs (PUT, DELETE) as needed
}
