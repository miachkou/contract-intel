using Application.Interfaces;
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
    private readonly IRiskScoringService _riskScoringService;
    private readonly ILogger<ClausesController> _logger;

    public ClausesController(
        ContractIntelDbContext context,
        IRiskScoringService riskScoringService,
        ILogger<ClausesController> logger)
    {
        _context = context;
        _riskScoringService = riskScoringService;
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
                    ExtractedAt = c.ExtractedAt,
                    ApprovedBy = c.ApprovedBy,
                    ApprovedAt = c.ApprovedAt
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

    [HttpPatch("{clauseId}")]
    public async Task<ActionResult<ClauseResponse>> UpdateClause(
        Guid contractId,
        Guid clauseId,
        [FromBody] UpdateClauseRequest request,
        CancellationToken cancellationToken = default)
    {
        if (contractId == Guid.Empty)
            return BadRequest(new { error = "Invalid contract ID" });

        if (clauseId == Guid.Empty)
            return BadRequest(new { error = "Invalid clause ID" });

        if (request.ClauseType != null && string.IsNullOrWhiteSpace(request.ClauseType))
            return BadRequest(new { error = "Clause type cannot be empty" });

        if (request.Excerpt != null && string.IsNullOrWhiteSpace(request.Excerpt))
            return BadRequest(new { error = "Excerpt cannot be empty" });

        try
        {
            var clause = await _context.Clauses
                .FirstOrDefaultAsync(c => c.Id == clauseId && c.ContractId == contractId, cancellationToken);

            if (clause == null)
                return NotFound(new { error = "Clause not found" });

            // Update fields
            bool updated = false;

            if (request.ClauseType != null)
            {
                clause.ClauseType = request.ClauseType;
                updated = true;
            }

            if (request.Excerpt != null)
            {
                clause.Excerpt = request.Excerpt;
                updated = true;
            }

            if (request.Approved == true && clause.ApprovedAt == null)
            {
                clause.ApprovedBy = request.ApprovedBy ?? "System";
                clause.ApprovedAt = DateTime.UtcNow;
                updated = true;
            }

            if (!updated)
                return BadRequest(new { error = "No valid fields to update" });

            await _context.SaveChangesAsync(cancellationToken);

            // Recompute risk score for the contract
            await RecomputeContractRisk(contractId, cancellationToken);

            // Return updated clause
            var updatedClause = await _context.Clauses
                .AsNoTracking()
                .FirstAsync(c => c.Id == clauseId, cancellationToken);

            return Ok(new ClauseResponse
            {
                Id = updatedClause.Id,
                ClauseType = updatedClause.ClauseType,
                Excerpt = updatedClause.Excerpt,
                Confidence = updatedClause.Confidence,
                PageNumber = updatedClause.PageNumber,
                ExtractedAt = updatedClause.ExtractedAt,
                ApprovedBy = updatedClause.ApprovedBy,
                ApprovedAt = updatedClause.ApprovedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update clause {ClauseId} for contract {ContractId}",
                clauseId,
                contractId);
            return StatusCode(500, new { error = "Failed to update clause" });
        }
    }

    private async Task RecomputeContractRisk(Guid contractId, CancellationToken cancellationToken)
    {
        var contract = await _context.Contracts
            .Include(c => c.Clauses)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract != null)
        {
            contract.RiskScore = _riskScoringService.CalculateRiskScore(contract.Clauses);
            contract.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
