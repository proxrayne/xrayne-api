using System.Runtime.CompilerServices;
using System.Text;

namespace RemoteNode.Parsing;

/// <summary>
/// Parses data payloads from a server-sent event stream.
/// </summary>
public static class ServerSentEventParser
{
    /// <summary>
    /// Reads SSE data payloads from a text stream.
    /// </summary>
    public static async IAsyncEnumerable<string> ReadDataAsync(
        TextReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var data = new StringBuilder();

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break;
            }

            if (line.Length == 0)
            {
                if (data.Length > 0)
                {
                    if (data[^1] == '\n')
                    {
                        data.Length--;
                    }

                    yield return data.ToString();
                    data.Clear();
                }

                continue;
            }

            if (line.StartsWith(':'))
            {
                continue;
            }

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                var value = line["data:".Length..];
                if (value.StartsWith(' '))
                {
                    value = value[1..];
                }

                data.Append(value);
                data.Append('\n');
            }
        }
    }
}
