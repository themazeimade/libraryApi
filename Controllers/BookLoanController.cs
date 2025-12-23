using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookLoansController : ControllerBase
{
    private readonly MongoDbService _mongo;

    public BookLoansController(MongoDbService mongo)
    {
        _mongo = mongo;
    }
    // Example GET endpoint (empty for now)
    [HttpGet]
    public async Task<IActionResult> GetBookLoans()
    {
        var bookLoan = await _mongo.BookLoans.Find(_ => true).ToListAsync();
        return Ok(bookLoan);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBookLoan(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid bookLoan id");

        var bookLoan = await _mongo.BookLoans
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (bookLoan == null)
            return NotFound();

        return Ok(bookLoan);
    }

    // Example POST endpoint (empty for now)
    [HttpPost]
    public async Task<IActionResult> CreateBookLoan([FromBody] BookLoanModel bookLoan)
    {

        Func<Task<IActionResult>> func = async () =>
        {
            //Validate the BookLoan is today
            if (DateTime.UtcNow.Date != bookLoan.BorrowedAt.Date)
            {
                var errors = new Dictionary<string, string[]>
                {
                  { "BorrowedAt", new[] { "BorrowedAt date must be today." } }
                };

                return ValidationProblem(new ValidationProblemDetails(errors));
            }
            //Validate ReturnDate
            if (DateTime.UtcNow.Date > bookLoan.DueDate.Date)
            {
                var errors = new Dictionary<string, string[]>
                {
                  { "DueDate", new[] { "DueDate date must be in the future." } }
                };

                return ValidationProblem(new ValidationProblemDetails(errors));
            }
            //Validate Id from User and BookCopy
            if (!ObjectId.TryParse(bookLoan.UserId, out var _))
                return BadRequest("Invalid user id");

            var user = await _mongo.Users
                .Find(b => b.Id == bookLoan.UserId)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();

            if (!ObjectId.TryParse(bookLoan.BookCopyId, out var _))
                return BadRequest("Invalid bookCopy id");

            var bookcopy = await _mongo.BookCopies
                .Find(b => b.Id == bookLoan.BookCopyId)
                .FirstOrDefaultAsync();

            if (bookcopy == null)
                return NotFound();


            //continue with procedure
            await _mongo.BookLoans.InsertOneAsync(bookLoan);

            //Update book copy availability
            var filter = Builders<BookCopyModel>.Filter.Eq(c => c.Id, bookLoan.BookCopyId);
            var update = Builders<BookCopyModel>.Update.Set(c => c.Available, false);

            var result = await _mongo.BookCopies.UpdateOneAsync(filter, update);

            return CreatedAtAction(nameof(GetBookLoan), new { id = bookLoan.Id }, bookLoan);

        };

        return await _mongo.SubmitMultipleTransaction(func);
    }


    [HttpPost("return/{bookcopy_id}")]
    public async Task<IActionResult> ReturnBook(string bookcopy_id)
    {
        if (!ObjectId.TryParse(bookcopy_id, out var _))
            return BadRequest("Invalid user id");

        var bookcopy = await _mongo.BookCopies
            .Find(b => b.Id == bookcopy_id)
            .FirstOrDefaultAsync();

        if (bookcopy == null)
            return NotFound();

        if (bookcopy.Available == true)
            return BadRequest("BookCopy is not loaned");

        var filterA = Builders<BookLoanModel>.Filter.And(
            Builders<BookLoanModel>.Filter.Eq(c => c.BookCopyId, bookcopy_id),
            Builders<BookLoanModel>.Filter.Eq(c => c.Status, "active")
        );
        var updateA = Builders<BookLoanModel>.Update.Set(c => c.Status, "Returned")
          .Set(c => c.ReturnedAt, DateTime.UtcNow);

        var result_a = await _mongo.BookLoans.FindOneAndUpdateAsync(
            filterA,
            updateA,
            new FindOneAndUpdateOptions<BookLoanModel>
            {
                ReturnDocument = ReturnDocument.After
            }
        );
        if (result_a == null)
            return NotFound();

        var filterB = Builders<BookCopyModel>.Filter.Eq(c => c.Id, bookcopy_id);
        var updateB = Builders<BookCopyModel>.Update.Set(c => c.Available, true);

        var result_b = await _mongo.BookCopies.FindOneAndUpdateAsync(
            filterB,
            updateB,
            new FindOneAndUpdateOptions<BookCopyModel>
            {
                ReturnDocument = ReturnDocument.After
            }
        );

        if (result_a == null || result_b == null)
            return NotFound();

        return Ok(new { result_a, result_b });

    }

    [HttpPost("duedate/{bookcopy_id}/{new_duedate:datetime}")]
    public async Task<IActionResult> ChangeDueDate(string bookcopy_id, DateTime new_duedate)
    {
        if (!ObjectId.TryParse(bookcopy_id, out var _))
            return BadRequest("Invalid user id");

        var bookcopy = await _mongo.BookCopies
            .Find(b => b.Id == bookcopy_id)
            .FirstOrDefaultAsync();

        if (bookcopy == null)
            return NotFound();

        if (bookcopy.Available == true)
            return BadRequest("BookCopy is not loaned");

        //!!!!!
        if (new_duedate.Date.AddDays(1) <= DateTime.UtcNow)
            return BadRequest("User can only change dueDate two days before current duedate");

        var filterA = Builders<BookLoanModel>.Filter.And(
            Builders<BookLoanModel>.Filter.Eq(c => c.BookCopyId, bookcopy_id),
            Builders<BookLoanModel>.Filter.Eq(c => c.Status, "active")
        );
        var UpdateA = Builders<BookLoanModel>.Update.Set(c => c.DueDate, new_duedate);

        var res = _mongo.BookLoans.FindOneAndUpdateAsync(
              filterA,
              UpdateA,
              new FindOneAndUpdateOptions<BookLoanModel>
              {
                  ReturnDocument = ReturnDocument.After
              }
        );

        if (res == null) return NotFound();

        return Ok(res);
    }

    [HttpPost("overdue/{bookcopy_id}")]
    public async Task<IActionResult> SetToOverdue(string bookcopy_id)
    {
        if (!ObjectId.TryParse(bookcopy_id, out var _))
            return BadRequest("Invalid user id");

        var bookcopy = await _mongo.BookCopies
            .Find(b => b.Id == bookcopy_id)
            .FirstOrDefaultAsync();

        if (bookcopy == null)
            return NotFound("BookCopyId not valid");

        if (bookcopy.Available == true)
            return BadRequest("BookCopy is not loaned");

        var filterA = Builders<BookLoanModel>.Filter.And(
            Builders<BookLoanModel>.Filter.Eq(c => c.BookCopyId, bookcopy_id),
            Builders<BookLoanModel>.Filter.Eq(c => c.Status, "active"),
            Builders<BookLoanModel>.Filter.Lt(c => c.DueDate, DateTime.UtcNow.Date.AddDays(1))
        );
        var UpdateA = Builders<BookLoanModel>.Update.Set(c => c.Status, "overdue");

        var res = _mongo.BookLoans.FindOneAndUpdateAsync(
              filterA,
              UpdateA,
              new FindOneAndUpdateOptions<BookLoanModel>
              {
                  ReturnDocument = ReturnDocument.After
              }
        );
        if (res == null) return NotFound();

        return Ok(res);
    }

    // [HttpPut("{id}")]
    // public async Task<IActionResult> UpdateBookLoan(string id, [FromBody] BookLoanModel bookLoan)
    // {
    //     if (!ObjectId.TryParse(id, out var _))
    //         return BadRequest("Invalid bookLoan id");
    //
    //     bookLoan.Id = id;
    //
    //     var res = await _mongo.BookLoans.ReplaceOneAsync(s => s.Id == bookLoan.Id, bookLoan);
    //
    //     if (res.MatchedCount == 0) return NotFound();
    //
    //     return Ok(res);
    // }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBookLoan(string id)
    {

        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid bookLoan id");

        var res = await _mongo.BookLoans.DeleteOneAsync(s => s.Id == id);

        if (res.DeletedCount == 0) return NotFound();

        return Ok(res);
    }

    // You can add other HTTP verbs (PUT, DELETE) as needed
}
