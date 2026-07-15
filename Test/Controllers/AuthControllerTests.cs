using Api.Controllers;
using Api.Exceptions;
using Api.Mapping;
using Api.Requests;
using AutoMapper;
using Contracts.Configurations;
using Data.Contracts;
using AdminAccount = Data.Entities.AdminAccount;
using Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace Test.Controllers;

/// <summary>
/// Tests administrator authentication endpoints.
/// </summary>
public sealed class AuthControllerTests
{
    private readonly IAdminAccountRepository _adminAccounts = Substitute.For<IAdminAccountRepository>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly IMapper _mapper = MapperTestFactory.CreateConfiguration(cfg => cfg.AddProfile<AdminMappingProfile>()).CreateMapper();

    [Fact]
    public async Task Login_ThrowsBadRequest_WhenAdminIsDeleted()
    {
        var controller = CreateController();
        _adminAccounts.GetActiveByUsernameAsync("admin", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AdminAccount?>(null));

        var request = new LoginRequest
        {
            Username = "admin",
            Password = "password"
        };

        var act = () => controller.Login(request, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid username or password.*");
    }

    private AuthController CreateController()
    {
        return new AuthController(
            _adminAccounts,
            _jwtTokenService,
            Options.Create(new JwtOptions
            {
                AccessTokenLifetimeMinutes = 15
            }),
            _mapper);
    }
}
