using System.CommandLine;
using XRayne.Cli.Commands.Xray;

namespace XRayne.Cli.Commands;

public sealed class RootCommandFactory(XrayCommandFactory xrayCommandFactory)
{
    public RootCommand Create()
    {
        var rootCommand = new RootCommand("XRayne CLI");
        
        rootCommand.Add(xrayCommandFactory.Create());

        return rootCommand;
    }
}
