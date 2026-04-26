using System.CommandLine;
using XRayne.Cli.Commands.Xray;

namespace XRayne.Cli.Commands;

public sealed class RootCommandFactory(XrayCommandFactory xrayCommandFactory)
{
    public RootCommand Create()
    {
        return new RootCommand("XRayne CLI")
        {
            xrayCommandFactory.Create()
        };
    }
}
