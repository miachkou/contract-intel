using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

#if DEBUG
/// <summary>
/// Development-only endpoints for data management.
/// Only compiled in Debug builds.
/// </summary>
[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    private readonly ContractIntelDbContext _dbContext;
    private readonly ILogger<DevController> _logger;
    private readonly IWebHostEnvironment _environment;

    public DevController(
        ContractIntelDbContext dbContext,
        ILogger<DevController> logger,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Seed the database with sample data. Only available in Development environment.
    /// </summary>
    /// <param name="reset">If true, clears existing data before seeding (default: false)</param>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedData([FromQuery] bool reset = false)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            if (reset)
            {
                _logger.LogWarning("Resetting database before seeding...");
                await _dbContext.Database.EnsureDeletedAsync();
                await _dbContext.Database.EnsureCreatedAsync();
            }

            await DevDataSeeder.SeedAsync(_dbContext, _logger);

            return Ok(new
            {
                message = "Seed data processed successfully",
                reset = reset
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed data");
            return StatusCode(500, new { error = "Failed to seed data", details = ex.Message });
        }
    }

    /// <summary>
    /// Reset the database (delete and recreate). Only available in Development environment.
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetDatabase()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        try
        {
            _logger.LogWarning("Resetting database...");
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();

            return Ok(new { message = "Database reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reset database");
            return StatusCode(500, new { error = "Failed to reset database", details = ex.Message });
        }
    }
}
#endif
