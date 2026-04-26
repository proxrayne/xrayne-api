namespace XRayne.Cli.Output;

public sealed class CliConsole : ICliConsole
{
    public void Success(string message)
    {
        WriteLine(ConsoleColor.Green, $"[OK] {message}");
    }

    public void Error(string message)
    {
        WriteLine(ConsoleColor.Red, $"[ERROR] {message}");
    }

    private static void WriteLine(ConsoleColor color, string message)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = previousColor;
    }
}
