using System.Collections.Concurrent;
using System.Text;

namespace Repositories.Utilities;

internal static class ConfigFileLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> LocalLocks = new(StringComparer.OrdinalIgnoreCase);
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public static async Task<IDisposable> AcquireAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var normalizedPath = Path.GetFullPath(path);
        var localLock = LocalLocks.GetOrAdd(normalizedPath, _ => new SemaphoreSlim(1, 1));

        if (!await localLock.WaitAsync(DefaultTimeout, cancellationToken))
        {
            throw new TimeoutException($"Could not acquire local config lock for '{normalizedPath}'.");
        }

        FileStream? lockFile = null;
        try
        {
            lockFile = await AcquireLockFileAsync(normalizedPath, cancellationToken);

            return new Releaser(localLock, lockFile);
        }
        catch
        {
            lockFile?.Dispose();
            localLock.Release();
            throw;
        }
    }

    public static async Task WriteAllTextAtomicAsync(
        string path,
        string content,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = Path.Combine(
            directory ?? string.Empty,
            $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await File.WriteAllTextAsync(temporaryPath, content, Encoding.UTF8, cancellationToken);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public static async Task WriteAllLinesAtomicAsync(
        string path,
        IEnumerable<string> lines,
        CancellationToken cancellationToken = default)
    {
        var content = string.Join(Environment.NewLine, lines) + Environment.NewLine;

        await WriteAllTextAtomicAsync(path, content, cancellationToken);
    }

    private static async Task<FileStream> AcquireLockFileAsync(
        string normalizedPath,
        CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(normalizedPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var lockPath = $"{normalizedPath}.lock";
        var startedAt = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - startedAt < DefaultTimeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return new FileStream(
                    lockPath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    bufferSize: 1,
                    FileOptions.DeleteOnClose);
            }
            catch (IOException)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
            }
            catch (UnauthorizedAccessException)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken);
            }
        }

        throw new TimeoutException($"Could not acquire config lock for '{normalizedPath}'.");
    }

    private sealed class Releaser : IDisposable
    {
        private readonly SemaphoreSlim _localLock;
        private readonly FileStream _lockFile;
        private bool _disposed;

        public Releaser(
            SemaphoreSlim localLock,
            FileStream lockFile)
        {
            _localLock = localLock;
            _lockFile = lockFile;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _lockFile.Dispose();
            _localLock.Release();
            _disposed = true;
        }
    }
}
