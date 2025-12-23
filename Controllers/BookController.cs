using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly MongoDbService _mongo;

    public BooksController(MongoDbService mongo)
    {
        _mongo = mongo;
    }
    // Example GET endpoint (empty for now)
    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _mongo.Books.Find(_ => true).ToListAsync();
        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid book id");

        var book = await _mongo.Books
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (book == null)
            return NotFound();

        return Ok(book);
    }

    // Example POST endpoint (empty for now)
    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] BookModel book)
    {
        await _mongo.Books.InsertOneAsync(book);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(string id, [FromBody] BookModel book)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid book id");

        book.Id = id;

        var res = await _mongo.Books.ReplaceOneAsync(s => s.Id == book.Id, book);

        if (res.MatchedCount == 0) return NotFound();

        // var book = await _mongo.Books
        //     .Find(b => b.Id == id)
        //     .FirstOrDefaultAsync();

        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(string id)
    {

        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid book id");

        var res = await _mongo.Books.DeleteOneAsync(s => s.Id == id);

        if (res.DeletedCount == 0) return NotFound();

        return Ok(res);
    }

    // You can add other HTTP verbs (PUT, DELETE) as needed
}
