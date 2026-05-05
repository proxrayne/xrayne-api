using System.Diagnostics;

namespace XRayne.Cli.Services;

public sealed class ShellService : IShellService
{
    public Task<string> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        return RunAsync(startInfo, $"{fileName} {arguments}", cancellationToken);
    }

    public Task<string> RunAsync(
        string fileName,
        IReadOnlyCollection<string> arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return RunAsync(startInfo, $"{fileName} {string.Join(' ', arguments)}", cancellationToken);
    }

    private static async Task<string> RunAsync(
        ProcessStartInfo startInfo,
        string command,
        CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var output = string.Join(Environment.NewLine, await stdout, await stderr).Trim();
        if (process.ExitCode == 0)
        {
            return output;
        }

        throw new InvalidOperationException($"{command} failed with exit code {process.ExitCode}.{Environment.NewLine}{output}");
    }
}
