using System.Security.Claims;
using Api.Controllers;
using Api.Exceptions;
using Api.Mapping;
using Api.Requests;
using Contracts.Enums;
using AutoMapper;
using Data.Contracts;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Test.Controllers;

/// <summary>
/// Tests administrator management endpoints.
/// </summary>
public sealed class AdminsControllerTests
{
    private static readonly Guid CurrentAdminId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    private readonly IAdminAccountRepository _adminAccounts = Substitute.For<IAdminAccountRepository>();
    private readonly IMapper _mapper = MapperTestFactory.CreateConfiguration(cfg => cfg.AddProfile<AdminMappingProfile>()).CreateMapper();

    [Fact]
    public async Task Delete_ThrowsBadRequest_WhenDeletingCurrentAdmin()
    {
        var controller = CreateController();

        var act = () => controller.Delete(CurrentAdminId, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Cannot delete the current administrator account.*");
        await _adminAccounts.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_ThrowsNotFound_WhenAdminMissingOrDeleted()
    {
        var controller = CreateController();
        _adminAccounts.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var act = () => controller.Delete(Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenAdminSoftDeleted()
    {
        var controller = CreateController();
        _adminAccounts.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await controller.Delete(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task ChangePassword_ThrowsNotFound_WhenAdminMissingOrDeleted()
    {
        var controller = CreateController();
        _adminAccounts.ChangePasswordAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((AdminAccount?)null);

        var act = () => controller.ChangePassword(
            Guid.NewGuid(),
            new ChangeAdminPasswordRequest { Password = "new-password" },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ChangePermissions_ThrowsNotFound_WhenAdminMissingOrDeleted()
    {
        var controller = CreateController();
        _adminAccounts.ChangePermissionsAsync(Arg.Any<Guid>(), Arg.Any<AdminPermission>(), Arg.Any<CancellationToken>())
            .Returns((AdminAccount?)null);

        var act = () => controller.ChangePermissions(
            Guid.NewGuid(),
            new ChangeAdminPermissionsRequest { Permissions = "view_logs" },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private AdminsController CreateController()
    {
        var controller = new AdminsController(_adminAccounts, _mapper)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, CurrentAdminId.ToString())],
                        "Test"))
                }
            }
        };

        return controller;
    }
}
