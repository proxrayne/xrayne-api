using Microsoft.Extensions.Logging.Abstractions;
using XRayne.Contracts.Configurations;
using XRayne.Infrastructure.Services.PanelSettings;

namespace XRayne.Test.Services.PanelSettings;

public sealed class PanelSettingsBootstrapServiceTests
{
    [Fact]
    public async Task StartAsync_CallsRefresh_AndClearsPendingRestart_WhenSet()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions());
        accessor.PendingRestart.Returns(true);

        var service = new PanelSettingsBootstrapService(accessor, NullLogger<PanelSettingsBootstrapService>.Instance);

        await service.StartAsync(CancellationToken.None);

        await accessor.Received(1).RefreshFromStoreAsync(Arg.Any<CancellationToken>());
        await accessor.Received(1).ClearPendingRestartAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartAsync_DoesNotClearPending_WhenAlreadyFalse()
    {
        var accessor = Substitute.For<IPanelSettingsAccessor>();
        accessor.Current.Returns(new PanelOptions());
        accessor.PendingRestart.Returns(false);

        var service = new PanelSettingsBootstrapService(accessor, NullLogger<PanelSettingsBootstrapService>.Instance);

        await service.StartAsync(CancellationToken.None);

        await accessor.Received(1).RefreshFromStoreAsync(Arg.Any<CancellationToken>());
        await accessor.DidNotReceive().ClearPendingRestartAsync(Arg.Any<CancellationToken>());
    }
}
