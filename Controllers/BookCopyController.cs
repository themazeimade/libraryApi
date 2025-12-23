using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookCopiesController : ControllerBase
{
    private readonly MongoDbService _mongo;

    public BookCopiesController(MongoDbService mongo)
    {
        _mongo = mongo;
    }
    // Example GET endpoint (empty for now)
    [HttpGet]
    public async Task<IActionResult> GetBookCopies()
    {
        var bookCopies = await _mongo.BookCopies.Find(_ => true).ToListAsync();
        return Ok(bookCopies);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookCopy(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid bookCopy id");

        var bookCopy = await _mongo.BookCopies
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (bookCopy == null)
            return NotFound();

        return Ok(bookCopy);
    }

    // Example POST endpoint (empty for now)
    [HttpPost]
    public async Task<IActionResult> CreateBookCopy([FromBody] BookCopyModel bookCopy)
    {
        await _mongo.BookCopies.InsertOneAsync(bookCopy);
        return CreatedAtAction(nameof(GetBookCopy), new { id = bookCopy.Id }, bookCopy);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBookCopy(string id, [FromBody] BookCopyModel bookCopy)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid bookCopy id");

        bookCopy.Id = id;

        var res = await _mongo.BookCopies.ReplaceOneAsync(s => s.Id == bookCopy.Id, bookCopy);

        if (res.MatchedCount == 0) return NotFound();

        // var bookCopy = await _mongo.bookCopyCopys
        //     .Find(b => b.Id == id)
        //     .FirstOrDefaultAsync();

        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBookCopy(string id)
    {

        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid bookCopy id");

        var res = await _mongo.BookCopies.DeleteOneAsync(s => s.Id == id);

        if (res.DeletedCount == 0) return NotFound();

        return Ok(res);
    }

    // You can add other HTTP verbs (PUT, DELETE) as needed
}
