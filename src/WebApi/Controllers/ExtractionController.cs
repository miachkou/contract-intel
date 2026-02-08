using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/contracts/{contractId}/extract")]
public class ExtractionController : ControllerBase
{
    private readonly IContractExtractionService _extractionService;
    private readonly ILogger<ExtractionController> _logger;

    public ExtractionController(
        IContractExtractionService extractionService,
        ILogger<ExtractionController> logger)
    {
        _extractionService = extractionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ExtractionSummary>> ExtractContract(
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        if (contractId == Guid.Empty)
            return BadRequest(new { error = "Invalid contract ID" });

        try
        {
            var summary = await _extractionService.ProcessContractAsync(
                contractId,
                cancellationToken);

            _logger.LogInformation(
                "Extraction completed for contract {ContractId}: {ClauseCount} clauses, risk {RiskScore}",
                contractId,
                summary.TotalClauses,
                summary.RiskScore);

            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex,
                "Extraction failed for contract {ContractId}: {Message}",
                contractId,
                ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error during extraction for contract {ContractId}",
                contractId);
            return StatusCode(500, new { error = "Failed to process contract extraction" });
        }
    }
}
