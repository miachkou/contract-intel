using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/contracts/{contractId}/clauses")]
public class ClausesController : ControllerBase
{
    private readonly ContractIntelDbContext _context;
    private readonly ILogger<ClausesController> _logger;

    public ClausesController(
        ContractIntelDbContext context,
        ILogger<ClausesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClauseResponse>>> GetClauses(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        if (contractId == Guid.Empty)
            return BadRequest(new { error = "Invalid contract ID" });

        try
        {
            var clauses = await _context.Clauses
                .AsNoTracking()
                .Where(c => c.ContractId == contractId)
                .OrderBy(c => c.PageNumber)
                .ThenBy(c => c.ClauseType)
                .Select(c => new ClauseResponse
                {
                    Id = c.Id,
                    ClauseType = c.ClauseType,
                    Excerpt = c.Excerpt,
                    Confidence = c.Confidence,
                    PageNumber = c.PageNumber,
                    ExtractedAt = c.ExtractedAt
                })
                .ToListAsync(cancellationToken);

            return Ok(clauses);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve clauses for contract {ContractId}",
                contractId);
            return StatusCode(500, new { error = "Failed to retrieve clauses" });
        }
    }
}
