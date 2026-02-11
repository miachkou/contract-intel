using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(string rootPath, ILogger<LocalFileStorageService> logger)
    {
        _rootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
            _logger.LogInformation("Created storage root directory: {RootPath}", _rootPath);
        }
    }

    public async Task<string> SaveFileAsync(Guid contractId, string fileName, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var contractFolder = Path.Combine(_rootPath, contractId.ToString());
        if (!Directory.Exists(contractFolder))
        {
            Directory.CreateDirectory(contractFolder);
        }

        var relativePath = Path.Combine(contractId.ToString(), fileName);
        var fullPath = Path.Combine(_rootPath, relativePath);

        using var fileStreamOut = File.Create(fullPath);
        await fileStream.CopyToAsync(fileStreamOut, cancellationToken);

        _logger.LogInformation("Saved file: {RelativePath}", relativePath);
        return relativePath;
    }

    public Task<Stream> OpenFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {relativePath}", fullPath);
        }

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task<bool> FileExistsAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(relativePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file: {RelativePath}", relativePath);
        }

        return Task.CompletedTask;
    }

    public string GetFullPath(string relativePath)
    {
        return Path.Combine(_rootPath, relativePath);
    }
}
