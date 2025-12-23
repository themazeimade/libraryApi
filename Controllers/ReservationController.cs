using Microsoft.AspNetCore.Mvc;
using LibraryApi.Services;
using LibraryApi.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace LibraryApi.Controllers;

public class ReservationsQuery
{
    // Filtering
    public int? roomSizeBottom { get; set; }
    public int? roomSizeTop { get; set; }
    public DateTime timeRangeBottom { get; set; } = DateTime.UtcNow;
    public DateTime timeRangeTop { get; set; } = DateTime.UtcNow.Date.AddDays(4);
}

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly MongoDbService _mongo;

    public ReservationsController(MongoDbService mongo)
    {
        _mongo = mongo;
    }
    // Example GET endpoint (empty for now)
    [HttpGet]
    public async Task<IActionResult> GetReservations()
    {
        var reservation = await _mongo.Reservations.Find(_ => true).ToListAsync();
        return Ok(reservation);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservation(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid reservation id");

        var reservation = await _mongo.Reservations
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (reservation == null)
            return NotFound();

        return Ok(reservation);
    }

    // Example POST endpoint (empty for now)
    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] RoomReservationModel reservation)
    {
        if (reservation.EndTime <= reservation.StartTime)
            return BadRequest("Reservation StartTime and EndTime don't make sense");

        if (Utilities.DateUtils.IsValidTimeStep(reservation.EndTime) | !Utilities.DateUtils.IsValidTimeStep(reservation.StartTime))
            return BadRequest("Reservation times don't comply with reservation time range step");

        if (reservation.StartTime > DateTime.UtcNow.Date.AddDays(5))
            return BadRequest("Reservation too far in the future");

        if (reservation.StartTime < Utilities.DateUtils.FloorTo30Minutes(DateTime.UtcNow))
            return BadRequest("Reservation is in the past");

        var conflictFilter =
            Builders<RoomReservationModel>.Filter.Eq(r => r.RoomId, reservation.RoomId)
            & Builders<RoomReservationModel>.Filter.In(
                r => r.Status,
                new[] { ReservationStatus.Active, ReservationStatus.OnCourse }
            )
            & Builders<RoomReservationModel>.Filter.Lt(r => r.StartTime, reservation.EndTime)
            & Builders<RoomReservationModel>.Filter.Gt(r => r.EndTime, reservation.StartTime);

        bool conflictExists = await _mongo.Reservations
            .Find(conflictFilter)
            .AnyAsync();

        if (conflictExists)
            return BadRequest("This room is already reserved for the selected time.");

        await _mongo.Reservations.InsertOneAsync(reservation);
        return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
    }

    [HttpPost("cancel/{id}")]
    public async Task<IActionResult> CancelReservation(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid reservation id");

        var reservation = await _mongo.Reservations
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (reservation == null)
            return NotFound("Reservation doesn't exist");

        if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Completed)
            return BadRequest("Reservation already Cancelled or Completed");


        var filter = (Builders<RoomReservationModel>.Filter.Eq(c => c.Status, ReservationStatus.Active)
          | Builders<RoomReservationModel>.Filter.Eq(c => c.Status, ReservationStatus.OnCourse))
          & Builders<RoomReservationModel>.Filter.Gt(c => c.EndTime, DateTime.UtcNow)
          & Builders<RoomReservationModel>.Filter.Eq(c => c.Id, id);
        var update = Builders<RoomReservationModel>.Update.Set(c => c.Status, ReservationStatus.Cancelled);

        var res = _mongo.Reservations.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<RoomReservationModel>
            {
                ReturnDocument = ReturnDocument.After
            }
        );

        if (res == null) return NotFound("Cancelation did not meet requirements or invalid id");

        return Ok(res);

    }

    [HttpPost("activate/{id}")]
    public async Task<IActionResult> ActivateReservation(string id)
    {
        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid reservation id");

        var reservation = await _mongo.Reservations
            .Find(b => b.Id == id)
            .FirstOrDefaultAsync();

        if (reservation == null)
            return NotFound("Reservation doesn't exist");

        if (reservation.Status == ReservationStatus.Cancelled || reservation.Status == ReservationStatus.Completed)
            return BadRequest("Reservation already Cancelled or Completed");


        var filter = Builders<RoomReservationModel>.Filter.Eq(c => c.Status, ReservationStatus.Active)
          & Builders<RoomReservationModel>.Filter.Lt(c => c.StartTime, DateTime.UtcNow)
          & Builders<RoomReservationModel>.Filter.Eq(c => c.Id, id);
        var update = Builders<RoomReservationModel>.Update.Set(c => c.Status, ReservationStatus.OnCourse);

        var res = _mongo.Reservations.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<RoomReservationModel>
            {
                ReturnDocument = ReturnDocument.After
            }
        );

        if (res == null) return NotFound("Activation did not meet requirements or invalid id");

        return Ok(res);

    }

    [HttpPost("availability/")]
    public async Task<IActionResult> CheckAvailability([FromQuery] ReservationsQuery query)
    {
        var rooms = await _mongo.StudyRooms
            .Find(r => r.Capacity >= query.roomSizeBottom && r.Capacity <= query.roomSizeTop)
            .ToListAsync();

        var reservations = await _mongo.Reservations.Find(r =>
            r.StartTime < query.timeRangeTop && r.EndTime > query.timeRangeBottom
            && (r.Status == ReservationStatus.Active | r.Status == ReservationStatus.OnCourse)
            ).ToListAsync();

        var reservationsByRoom = reservations.GroupBy(r => r.RoomId)
          .ToDictionary(g => g.Key, g => g.OrderBy(r => r.StartTime).ToList());

        var response = new List<object>();

        foreach (var room in rooms)
        {
            if (room.Id == null) continue;
            reservationsByRoom.TryGetValue(room.Id, out var roomReservations);
            roomReservations ??= new List<RoomReservationModel>();

            var availableIntervals = Utilities.DateUtils.GetAvailableIntervals(
                query.timeRangeBottom,
                query.timeRangeTop,
                roomReservations
            );

            if (availableIntervals.Any())
            {
                response.Add(new
                {
                    RoomId = room.Id,
                    Capacity = room.Capacity,
                    AvailableIntervals = availableIntervals.Select(i => new
                    {
                        StartTime = i.Start,
                        EndTime = i.End
                    })
                });
            }


        }

        return Ok(response);
    }

    [HttpPost("complete/")]
    public async Task<IActionResult> CompleteReservations()
    {

        var filter = Builders<RoomReservationModel>.Filter.Eq(c => c.Status, ReservationStatus.OnCourse)
          & Builders<RoomReservationModel>.Filter.Lt(c => c.EndTime, DateTime.UtcNow)
          ;
        var update = Builders<RoomReservationModel>.Update.Set(c => c.Status, ReservationStatus.Completed);

        var res = await _mongo.Reservations.UpdateManyAsync(filter, update);
        return Ok(res);
    }

    // [HttpPost("user/{reservation_id}")]
    // public async Task<IActionResult> CheckUserReservations(string reservation_id)
    // {
    //     return Ok();
    // }

    [HttpPost("automation/cancel")]
    public async Task<IActionResult> CancelNotActivatedReservations(string reservation_id)
    {
        var filter = Builders<RoomReservationModel>.Filter.Eq(c => c.Status, ReservationStatus.Active)
          & Builders<RoomReservationModel>.Filter.Lt(c => c.StartTime, DateTime.UtcNow.AddMinutes(-5));
        var update = Builders<RoomReservationModel>.Update.Set(c => c.Status, ReservationStatus.Cancelled);

        var res = await _mongo.Reservations.UpdateManyAsync(filter, update);


        var response = new
        {
            MatchedCount = res.MatchedCount,
            ModifiedCount = res.ModifiedCount,
            IsAcknowledged = res.IsAcknowledged
        };

        return Ok(response);

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(string id)
    {

        if (!ObjectId.TryParse(id, out var _))
            return BadRequest("Invalid reservation id");

        var res = await _mongo.Reservations.DeleteOneAsync(s => s.Id == id);

        if (res.DeletedCount == 0) return NotFound();

        return Ok(res);
    }

    // You can add other HTTP verbs (PUT, DELETE) as needed
}
