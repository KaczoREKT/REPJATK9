namespace DefaultNamespace;

public class TripsController
{
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Models;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly TripsDbContext _context;

    public TripsController(TripsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Name,
                t.Description,
                t.DateFrom,
                t.DateTo,
                t.MaxPeople,
                Countries = t.TripCountries.Select(tc => new { tc.Country.Name }).ToList(),
                Clients = t.ClientTrips.Select(ct => new { ct.Client.FirstName, ct.Client.LastName }).ToList()
            })
            .ToListAsync();

        var totalTrips = await _context.Trips.CountAsync();

        return Ok(new
        {
            pageNum = page,
            pageSize,
            allPages = (int)Math.Ceiling(totalTrips / (double)pageSize),
            trips
        });
    }
    [HttpPost("{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] ClientTripDto clientTripDto)
    {
        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.IdTrip == idTrip);

        if (trip == null)
        {
            return NotFound(new { message = "Trip not found" });
        }

        if (trip.DateFrom < DateTime.Now)
        {
            return BadRequest(new { message = "Cannot assign to a trip that has already started" });
        }

        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.Pesel == clientTripDto.Pesel);

        if (client != null)
        {
            var existingAssignment = await _context.ClientTrips
                .FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip);

            if (existingAssignment != null)
            {
                return BadRequest(new { message = "Client is already assigned to this trip" });
            }
        }
        else
        {
            client = new Client
            {
                FirstName = clientTripDto.FirstName,
                LastName = clientTripDto.LastName,
                Email = clientTripDto.Email,
                Telephone = clientTripDto.Telephone,
                Pesel = clientTripDto.Pesel
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = clientTripDto.PaymentDate
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
}