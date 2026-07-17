
using Contracts.Values;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class TempFileStorage(ILogger<TempFileStorage> logger) : ITempFileStorage
{
    public void Delete(string filepath, CancellationToken cancellationToken)
    {
        try
        {
            if (File.Exists(filepath)) File.Delete(filepath);
        }
        catch (IOException)
        {
            logger.LogWarning("Failed to delete temporary geo resource upload {UploadFilePath}.", filepath);
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("Access denied while deleting temporary geo resource upload {UploadFilePath}.", filepath);
        }
    }

    public async Task<string> WriteAsync(Stream content, CancellationToken cancellationToken)
    {
        var directory = PathProvider.Paths.Temp;
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var path = Path.Combine(directory, $"{Guid.NewGuid():N}.upload");

        await using var file = File.Create(path);
        await content.CopyToAsync(file, cancellationToken);

        return path;
    }
}