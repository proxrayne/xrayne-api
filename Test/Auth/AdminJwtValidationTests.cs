using System.Security.Claims;
using Api.Auth;
using Data.Contracts;
using Data.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Auth;

/// <summary>
/// Tests administrator JWT account-state validation.
/// </summary>
public sealed class AdminJwtValidationTests
{
    [Fact]
    public async Task ValidateActiveAdminAsync_Fails_WhenAdminIsDeleted()
    {
        var adminId = Guid.NewGuid();
        var adminAccounts = Substitute.For<IAdminAccountRepository>();
        adminAccounts.GetActiveByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns((AdminAccount?)null);
        var context = CreateContext(adminId, adminAccounts);

        await AdminJwtValidation.ValidateActiveAdminAsync(context);

        context.Result?.Failure.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateActiveAdminAsync_Succeeds_WhenAdminIsActive()
    {
        var adminId = Guid.NewGuid();
        var adminAccounts = Substitute.For<IAdminAccountRepository>();
        adminAccounts.GetActiveByIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(new AdminAccount
            {
                Id = adminId,
                Username = "admin",
                PasswordHash = "hash"
            });
        var context = CreateContext(adminId, adminAccounts);

        await AdminJwtValidation.ValidateActiveAdminAsync(context);

        context.Result.Should().BeNull();
    }

    private static TokenValidatedContext CreateContext(
        Guid adminId,
        IAdminAccountRepository adminAccounts)
    {
        var services = new ServiceCollection()
            .AddSingleton(adminAccounts)
            .BuildServiceProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, adminId.ToString())],
                JwtBearerDefaults.AuthenticationScheme))
        };
        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme,
            typeof(JwtBearerHandler));

        return new TokenValidatedContext(httpContext, scheme, new JwtBearerOptions())
        {
            Principal = httpContext.User
        };
    }
}
