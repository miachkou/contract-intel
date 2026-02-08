using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/contracts/{contractId}/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileStorageService fileStorage, ILogger<FilesController> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<object>> UploadFile(Guid contractId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        if (contractId == Guid.Empty)
            return BadRequest("Invalid contract ID");

        try
        {
            using var stream = file.OpenReadStream();
            var relativePath = await _fileStorage.SaveFileAsync(contractId, file.FileName, stream);

            return Ok(new
            {
                fileName = file.FileName,
                relativePath = relativePath,
                size = file.Length,
                contentType = file.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for contract {ContractId}", file.FileName, contractId);
            return StatusCode(500, "Failed to upload file");
        }
    }

    [HttpGet("{*relativePath}")]
    public async Task<IActionResult> DownloadFile(Guid contractId, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return BadRequest("File path is required");

        try
        {
            // Ensure the path belongs to the specified contract
            var expectedPrefix = $"{contractId}/";
            if (!relativePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file path for this contract");

            var exists = await _fileStorage.FileExistsAsync(relativePath);
            if (!exists)
                return NotFound("File not found");

            var stream = await _fileStorage.OpenFileAsync(relativePath);
            var fileName = Path.GetFileName(relativePath);

            return File(stream, "application/octet-stream", fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {RelativePath}", relativePath);
            return StatusCode(500, "Failed to download file");
        }
    }

    [HttpDelete("{*relativePath}")]
    public async Task<IActionResult> DeleteFile(Guid contractId, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return BadRequest("File path is required");

        try
        {
            // Ensure the path belongs to the specified contract
            var expectedPrefix = $"{contractId}/";
            if (!relativePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file path for this contract");

            var exists = await _fileStorage.FileExistsAsync(relativePath);
            if (!exists)
                return NotFound("File not found");

            await _fileStorage.DeleteFileAsync(relativePath);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {RelativePath}", relativePath);
            return StatusCode(500, "Failed to delete file");
        }
    }

    [HttpHead("{*relativePath}")]
    public async Task<IActionResult> CheckFile(Guid contractId, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return BadRequest("File path is required");

        var expectedPrefix = $"{contractId}/";
        if (!relativePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid file path for this contract");

        var exists = await _fileStorage.FileExistsAsync(relativePath);
        return exists ? Ok() : NotFound();
    }
}
