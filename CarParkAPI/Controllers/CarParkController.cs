using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarParkAPI.Data;
using CarParkAPI.Models;
using CarParkAPI.Services;

namespace CarParkAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarParkController(DataContext context, CarParkBatchService batchService) : ControllerBase
{
    public class ImportRequest { public string Data { get; set; } = null!; }

    [HttpPost("import-delta")]
    public async Task<IActionResult> ImportDeltaFile([FromBody] ImportRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Data)) return BadRequest("CSV data cannot be empty.");
        try
        {
            await batchService.ProcessDeltaFileAsync(request.Data);
            return Ok(new { message = "Daily delta metrics processed successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Batch failure detected. Fully rolled back.", details = ex.Message });
        }
    }

    [HttpGet("filter")]
    public async Task<IActionResult> FilterCarParks([FromQuery] bool? free, [FromQuery] bool? night, [FromQuery] decimal? height)
    {
        var query = context.CarParks.Include(c => c.CarParkType).Include(c => c.ParkingSystem).Include(c => c.ParkingPolicy).AsQueryable();

        if (free == true) query = query.Where(c => c.ParkingPolicy.FreeParking.ToUpper() != "NO");
        if (night.HasValue) query = query.Where(c => c.ParkingPolicy.OffersNightParking == night.Value);
        if (height.HasValue) query = query.Where(c => c.GantryHeight >= height.Value);

        return Ok(await query.ToListAsync());
    }

    [HttpPost("favorites")]
    public async Task<IActionResult> AddToFavorites([FromQuery] string userId, [FromQuery] string carParkNo)
    {
        if (!await context.CarParks.AnyAsync(c => c.CarParkNo == carParkNo))
            return NotFound($"Carpark '{carParkNo}' does not exist.");

        if (await context.UserFavorites.AnyAsync(f => f.UserId == userId && f.CarParkNo == carParkNo))
            return BadRequest("Location already saved in your favorites list.");

        context.UserFavorites.Add(new UserFavorite { UserId = userId, CarParkNo = carParkNo });
        await context.SaveChangesAsync();

        return Ok(new { message = $"Carpark '{carParkNo}' successfully added to user favorites." });
    }
}