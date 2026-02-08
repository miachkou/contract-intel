namespace Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file for a specific contract and returns the relative path to store in the database.
    /// </summary>
    Task<string> SaveFileAsync(Guid contractId, string fileName, Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a file stream for reading from the given relative path.
    /// </summary>
    Task<Stream> OpenFileAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists at the given relative path.
    /// </summary>
    Task<bool> FileExistsAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file at the given relative path.
    /// </summary>
    Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full physical path for a relative path (for debugging/logging purposes).
    /// </summary>
    string GetFullPath(string relativePath);
}
