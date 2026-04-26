namespace XRayne.Cli.Output;

public interface ICliConsole
{
    void Success(string message);

    void Error(string message);
}
