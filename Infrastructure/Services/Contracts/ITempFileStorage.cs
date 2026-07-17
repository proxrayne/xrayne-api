namespace Infrastructure.Services;

public interface ITempFileStorage
{
    /// <summary>
    /// Write temp file to storage.
    /// </summary>
    Task<string> WriteAsync(Stream content, CancellationToken cancellationToken);

    /// <summary>
    /// Delete temp file from storage.
    /// </summary>
    void Delete(string filepath, CancellationToken cancellationToken);
}