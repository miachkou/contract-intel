using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly ContractIntelDbContext _context;

    public ContractsController(ContractIntelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Contract>>> GetContracts()
    {
        return await _context.Contracts
            .Include(c => c.Documents)
            .Include(c => c.Clauses)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Contract>> GetContract(Guid id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Documents)
            .Include(c => c.Clauses)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
            return NotFound();

        return contract;
    }

    [HttpPost]
    public async Task<ActionResult<Contract>> CreateContract(Contract contract)
    {
        contract.Id = Guid.NewGuid();
        contract.CreatedAt = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, contract);
    }
}
