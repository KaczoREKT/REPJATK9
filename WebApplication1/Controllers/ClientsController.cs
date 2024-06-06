using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using YourNamespace.Models;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly TripsDbContext _context;

    public ClientsController(TripsDbContext context)
    {
        _context = context;
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == idClient);

        if (client == null)
        {
            return NotFound(new { message = "Client not found" });
        }

        if (client.ClientTrips.Any())
        {
            return BadRequest(new { message = "Client cannot be deleted because they are assigned to trips" });
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}