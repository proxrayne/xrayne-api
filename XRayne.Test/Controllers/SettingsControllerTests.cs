using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using XRayne.Api.Controllers;
using XRayne.Api.Mapping;
using XRayne.Api.Requests;
using XRayne.Api.Responses;
using XRayne.Contracts.Configurations;
using XRayne.Infrastructure.Services;

namespace XRayne.Test.Controllers;

public sealed class SettingsControllerTests
{
    private readonly IPanelSettingsAccessor _accessor;
    private readonly IPanelRestartService _restartService;
    private readonly IMapper _mapper;
    private readonly SettingsController _controller;

    public SettingsControllerTests()
    {
        _accessor = Substitute.For<IPanelSettingsAccessor>();
        _restartService = Substitute.For<IPanelRestartService>();
        _restartService.ScheduleRestart().Returns(true);
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<PanelSettingsProfile>()).CreateMapper();

        _controller = new SettingsController(
            _accessor,
            _mapper,
            _restartService,
            NullLogger<SettingsController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public void Get_ReturnsCurrentSettings_WithFieldImpacts_AndPendingRestart()
    {
        _accessor.Current.Returns(new PanelSettings { Port = 4242 });
        _accessor.PendingRestart.Returns(true);

        var response = _controller.GetCurrent();

        response.Port.Should().Be(4242);
        response.PendingRestart.Should().BeTrue();
        response.FieldImpacts.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(RestartImpact.None, false, 0)]
    [InlineData(RestartImpact.HotReload, false, 1)]
    [InlineData(RestartImpact.FullRestart, true, 0)]
    public async Task Update_ReturnsCorrectFlags(RestartImpact impact, bool expectedRestart, int expectedHotCount)
    {
        var changed = impact == RestartImpact.None ? Array.Empty<string>() : new[] { "WebBasePath" };
        _accessor.ApplyAsync(Arg.Any<PanelSettings>(), Arg.Any<CancellationToken>())
            .Returns(new SettingsApplyResult(changed, impact));

        var response = await _controller.UpdatePanel(new UpdatePanelSettingsRequest(), CancellationToken.None);

        response.RequiresRestart.Should().Be(expectedRestart);
        response.HotReloaded.Should().HaveCount(expectedHotCount);
    }

    [Fact]
    public async Task Update_CallsAccessor_WithMappedOptions()
    {
        _accessor.ApplyAsync(Arg.Any<PanelSettings>(), Arg.Any<CancellationToken>())
            .Returns(new SettingsApplyResult(Array.Empty<string>(), RestartImpact.None));

        var request = new UpdatePanelSettingsRequest { Port = 7070, PathBase = "/x/" };

        await _controller.UpdatePanel(request, CancellationToken.None);

        await _accessor.Received(1).ApplyAsync(
            Arg.Is<PanelSettings>(o => o.Port == 7070 && o.PathBase == "/x/"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Restart_Returns202_AndSchedulesRestart()
    {
        var result = _controller.RestartPanel();

        result.Should().BeOfType<AcceptedResult>();
        _restartService.Received(1).ScheduleRestart();
    }

    [Fact]
    public void Restart_Returns202_EvenIfAlreadyScheduled()
    {
        _restartService.ScheduleRestart().Returns(false);

        var result = _controller.RestartPanel();

        result.Should().BeOfType<AcceptedResult>();
    }
}
