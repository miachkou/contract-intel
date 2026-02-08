using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractRepository _contractRepository;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(
        IContractRepository contractRepository,
        ILogger<ContractsController> logger)
    {
        _contractRepository = contractRepository;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ContractResponse>> CreateContract(CreateContractRequest request)
    {
        try
        {
            var contract = new Contract
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Vendor = request.Vendor,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RenewalDate = request.RenewalDate,
                Status = request.Status ?? "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _contractRepository.AddAsync(contract);

            var response = new ContractResponse
            {
                Id = created.Id,
                Title = created.Title,
                Vendor = created.Vendor,
                StartDate = created.StartDate,
                EndDate = created.EndDate,
                RenewalDate = created.RenewalDate,
                RiskScore = created.RiskScore,
                Status = created.Status,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt
            };

            return CreatedAtAction(nameof(GetContract), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create contract");
            return StatusCode(500, new { error = "Failed to create contract" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContractResponse>>> GetContracts(
        [FromQuery] string? vendor = null,
        [FromQuery] decimal? minRisk = null,
        [FromQuery] DateTime? renewalBefore = null)
    {
        try
        {
            var contracts = await _contractRepository.SearchAsync(
                vendorContains: vendor,
                minRiskScore: minRisk,
                renewalBefore: renewalBefore,
                skip: 0,
                take: 100);

            var response = contracts.Select(c => new ContractResponse
            {
                Id = c.Id,
                Title = c.Title,
                Vendor = c.Vendor,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                RenewalDate = c.RenewalDate,
                RiskScore = c.RiskScore,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve contracts");
            return StatusCode(500, new { error = "Failed to retrieve contracts" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ContractResponse>> GetContract(Guid id)
    {
        try
        {
            var contract = await _contractRepository.GetByIdAsync(id);

            if (contract == null)
                return NotFound(new { error = "Contract not found" });

            var response = new ContractResponse
            {
                Id = contract.Id,
                Title = contract.Title,
                Vendor = contract.Vendor,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                RenewalDate = contract.RenewalDate,
                RiskScore = contract.RiskScore,
                Status = contract.Status,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve contract {ContractId}", id);
            return StatusCode(500, new { error = "Failed to retrieve contract" });
        }
    }
}
