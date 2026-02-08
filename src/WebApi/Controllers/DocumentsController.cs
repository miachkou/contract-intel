using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Controllers;

[ApiController]
[Route("api/contracts/{contractId}/documents")]
public class DocumentsController : ControllerBase
{
    private readonly ContractIntelDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        ContractIntelDbContext context,
        IFileStorageService fileStorage,
        ILogger<DocumentsController> logger)
    {
        _context = context;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<DocumentResponse>> UploadDocument(
        Guid contractId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file provided" });

        if (contractId == Guid.Empty)
            return BadRequest(new { error = "Invalid contract ID" });

        // Check if contract exists
        var contractExists = await _context.Contracts
            .AnyAsync(c => c.Id == contractId, cancellationToken);

        if (!contractExists)
            return NotFound(new { error = "Contract not found" });

        try
        {
            // Save file to storage
            string relativePath;
            using (var stream = file.OpenReadStream())
            {
                relativePath = await _fileStorage.SaveFileAsync(
                    contractId,
                    file.FileName,
                    stream,
                    cancellationToken);
            }

            // Create document record
            var document = new Document
            {
                Id = Guid.NewGuid(),
                ContractId = contractId,
                FileName = file.FileName,
                FilePath = relativePath,
                FileSize = file.Length,
                MimeType = file.ContentType,
                UploadedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Uploaded document {DocumentId} for contract {ContractId}",
                document.Id,
                contractId);

            var response = new DocumentResponse
            {
                Id = document.Id,
                FileName = document.FileName,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                UploadedAt = document.UploadedAt
            };

            return CreatedAtAction(
                nameof(GetDocument),
                new { contractId, documentId = document.Id },
                response);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to upload document for contract {ContractId}",
                contractId);
            return StatusCode(500, new { error = "Failed to upload document" });
        }
    }

    [HttpGet("{documentId}")]
    public async Task<ActionResult<DocumentResponse>> GetDocument(
        Guid contractId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _context.Documents
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    d => d.Id == documentId && d.ContractId == contractId,
                    cancellationToken);

            if (document == null)
                return NotFound(new { error = "Document not found" });

            var response = new DocumentResponse
            {
                Id = document.Id,
                FileName = document.FileName,
                FileSize = document.FileSize,
                MimeType = document.MimeType,
                UploadedAt = document.UploadedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve document {DocumentId}", documentId);
            return StatusCode(500, new { error = "Failed to retrieve document" });
        }
    }
}
