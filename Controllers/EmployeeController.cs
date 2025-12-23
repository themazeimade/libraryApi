using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly MongoDbService _mongo;

    public EmployeeController(MongoDbService mongo)
    {
        _mongo = mongo;
    }
    // Example GET endpoint (empty for now)
    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var employee = await _mongo.Employees.Find(_ => true).ToListAsync();
        return Ok(employee);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid employee id");

        var employee = await _mongo.Employees
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    // Example POST endpoint (empty for now)
    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeModel employee)
    {
        await _mongo.Employees.InsertOneAsync(employee);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(string id, [FromBody] EmployeeModel employee)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid employee id");

        employee.Id = id;

        var res = await _mongo.Employees.ReplaceOneAsync(s => s.Id == employee.Id, employee);

        if (res.MatchedCount == 0) return NotFound();

        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(string id)
    {

        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid employee id");

        var res = await _mongo.Employees.DeleteOneAsync(s => s.Id == id);

        if (res.DeletedCount == 0) return NotFound();

        return Ok(res);
    }

    // You can add other HTTP verbs (PUT, DELETE) as needed
}
