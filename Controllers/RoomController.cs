using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly MongoDbService _mongo;

    public RoomController(MongoDbService mongo)
    {
        _mongo = mongo;
    }
    // Example GET endpoint (empty for now)
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        var room = await _mongo.StudyRooms.Find(_ => true).ToListAsync();
        return Ok(room);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoom(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid room id");

        var room = await _mongo.StudyRooms
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (room == null)
            return NotFound();

        return Ok(room);
    }

    // Example POST endpoint (empty for now)
    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] RoomModel room)
    {
        await _mongo.StudyRooms.InsertOneAsync(room);
        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(string id, [FromBody] RoomModel room)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid room id");

        room.Id = id;

        var res = await _mongo.StudyRooms.ReplaceOneAsync(s => s.Id == room.Id, room);

        if (res.MatchedCount == 0) return NotFound();

        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(string id)
    {

        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid room id");

        var res = await _mongo.StudyRooms.DeleteOneAsync(s => s.Id == id);

        if (res.DeletedCount == 0) return NotFound();

        return Ok(res);
    }

    // You can add other HTTP verbs (PUT, DELETE) as needed
}
