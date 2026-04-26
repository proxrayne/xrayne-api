using System.CommandLine;

namespace XRayne.Cli.Commands.Xray;

public sealed class XrayCommandFactory(IServiceProvider serviceProvider)
{
    public Command Create()
    {
        var xrayCommand = new Command("xray", "Processing of the xray-core");

        xrayCommand.Add(XrayStartAction.Create(serviceProvider));

        return xrayCommand;
    }


}
