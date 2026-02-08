using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IContractRepository _contractRepository;

    public ContractsController(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Contract>>> GetContracts()
    {
        var contracts = await _contractRepository.GetAllAsync();
        return Ok(contracts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Contract>> GetContract(Guid id)
    {
        var contract = await _contractRepository.GetByIdWithDetailsAsync(id);

        if (contract == null)
            return NotFound();

        return Ok(contract);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Contract>>> SearchContracts(
        [FromQuery] string? vendor = null,
        [FromQuery] decimal? minRisk = null,
        [FromQuery] DateTime? renewalBefore = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var contracts = await _contractRepository.SearchAsync(
            vendorContains: vendor,
            minRiskScore: minRisk,
            renewalBefore: renewalBefore,
            skip: skip,
            take: take);

        return Ok(contracts);
    }

    [HttpGet("upcoming-renewals")]
    public async Task<ActionResult<IEnumerable<Contract>>> GetUpcomingRenewals(
        [FromQuery] int daysAhead = 90,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var contracts = await _contractRepository.GetUpcomingRenewalsAsync(
            daysAhead: daysAhead,
            skip: skip,
            take: take);

        return Ok(contracts);
    }

    [HttpPost]
    public async Task<ActionResult<Contract>> CreateContract(Contract contract)
    {
        contract.Id = Guid.NewGuid();
        contract.CreatedAt = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;

        var created = await _contractRepository.AddAsync(contract);

        return CreatedAtAction(nameof(GetContract), new { id = created.Id }, created);
    }
}
