using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarParkAPI.Data;
using CarParkAPI.Models;
using CarParkAPI.Services;

namespace CarParkAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarParkController : ControllerBase
{
    private readonly DataContext _context;
    private readonly CarParkBatchService _batchService;

    public CarParkController(DataContext context, CarParkBatchService batchService)
    {
        _context = context;
        _batchService = batchService;
    }

    /// <summary>
    /// Request carrier class to facilitate clean Swagger UI inputs
    /// </summary>
    public class ImportRequest
    {
        public string Data { get; set; } = null!;
    }

    /// <summary>
    /// Task 2.1: Daily Delta Batch Ingestion Interface Endpoint
    /// </summary>
    [HttpPost("import-delta")]
    public async Task<IActionResult> ImportDeltaFile([FromBody] ImportRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Data))
        {
            return BadRequest("CSV payload data cannot be empty.");
        }

        try
        {
            await _batchService.ProcessDeltaFileAsync(request.Data);
            return Ok(new { message = "Daily delta metrics processed and committed successfully." });
        }
        catch (Exception ex)
        {
            // The batch service handles the database rollback internally; we catch and report the failure here
            return StatusCode(500, new { error = "Batch failure detected. Operation fully aborted and rolled back.", details = ex.Message });
        }
    }

    /// <summary>
    /// User Story 1: Filter list of carparks by free parking, night parking, and vehicle clearance height
    /// </summary>
    [HttpGet("filter")]
    public async Task<IActionResult> FilterCarParks(
        [FromQuery] bool? offerFreeParking,
        [FromQuery] bool? offerNightParking,
        [FromQuery] decimal? minVehicleHeight)
    {
        // Use IQueryable to defer database execution and filter efficiently on SQLite
        IQueryable<CarPark> query = _context.CarParks
            .Include(c => c.CarParkType)
            .Include(c => c.ParkingSystem)
            .Include(c => c.ParkingPolicy);

        if (offerFreeParking.HasValue && offerFreeParking.Value)
        {
            // Filters out any rules explicitly marked as "NO" free parking
            query = query.Where(c => c.ParkingPolicy.FreeParking.ToUpper() != "NO");
        }

        if (offerNightParking.HasValue)
        {
            query = query.Where(c => c.ParkingPolicy.OffersNightParking == offerNightParking.Value);
        }

        if (minVehicleHeight.HasValue)
        {
            // Return spots that can safely clear or match the user's vehicle height specification
            query = query.Where(c => c.GantryHeight >= minVehicleHeight.Value);
        }

        var results = await query.ToListAsync();
        return Ok(results);
    }

    /// <summary>
    /// User Story 2: Save a specific carpark as a user favorite
    /// </summary>
    [HttpPost("favorites")]
    public async Task<IActionResult> AddToFavorites([FromQuery] string userId, [FromQuery] string carParkNo)
    {
        // Verify target carpark exists before adding to favorites
        var carParkExists = await _context.CarParks.AnyAsync(c => c.CarParkNo == carParkNo);
        if (!carParkExists)
        {
            return NotFound($"Carpark facility '{carParkNo}' does not exist in our systems.");
        }

        // Prevent duplicate favorite entries for the same user
        var alreadyFavorited = await _context.UserFavorites
            .AnyAsync(f => f.UserId == userId && f.CarParkNo == carParkNo);
            
        if (alreadyFavorited)
        {
            return BadRequest("This parking location is already saved in your favorites list.");
        }

        var favorite = new UserFavorite
        {
            UserId = userId,
            CarParkNo = carParkNo
        };

        _context.UserFavorites.Add(favorite);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Carpark facility '{carParkNo}' successfully added to user favorites." });
    }
}