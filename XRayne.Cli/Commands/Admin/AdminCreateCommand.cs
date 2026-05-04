using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XRayne.Cli.Output;
using XRayne.Core.Auth;
using XRayne.Infrastructure.Auth;
using XRayne.Repositories;
using XRayne.Repositories.Admins;
using XRayne.Repositories.Entities;

namespace XRayne.Cli.Commands.Admin;

public sealed class AdminCreateCommand : Command
{
    public AdminCreateCommand(IServiceProvider serviceProvider)
        : base("create", "Create an administrator account")
    {
        var usernameArgument = new Argument<string>("username")
        {
            Description = "Administrator username"
        };

        var passwordOption = new Option<string>("--password", ["-p"])
        {
            Description = "Administrator password",
            Required = true
        };

        var permissionsOption = new Option<string>("--permissions")
        {
            Description = "Comma-separated permissions: super_admin,create_users,edit_users,delete_users,reset_traffic,change_xray_settings,view_logs,manage_admins",
            DefaultValueFactory = _ => "super_admin"
        };

        Add(usernameArgument);
        Add(passwordOption);
        Add(permissionsOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var username = parseResult.GetRequiredValue(usernameArgument);
            var password = parseResult.GetRequiredValue(passwordOption);
            var permissions = parseResult.GetRequiredValue(permissionsOption);

            return await ExecuteAsync(
                scope.ServiceProvider,
                username,
                password,
                permissions,
                cancellationToken);
        });
    }

    private static async Task<int> ExecuteAsync(
        IServiceProvider serviceProvider,
        string username,
        string password,
        string permissionsValue,
        CancellationToken cancellationToken)
    {
        var adminAccounts = serviceProvider.GetRequiredService<IAdminAccountRepository>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
        var console = serviceProvider.GetRequiredService<ICliConsole>();
        var logger = serviceProvider.GetRequiredService<ILogger<AdminCreateCommand>>();

        try
        {
            await serviceProvider.MigrateDatabaseAsync(cancellationToken);

            var exists = await adminAccounts.ExistsAsync(username, cancellationToken);
            if (exists)
            {
                console.Error($"Admin account '{username}' already exists.");

                return 1;
            }

            var permissions = AdminPermissionNames.ParseMany(permissionsValue);
            var account = new AdminAccount
            {
                Username = username,
                PasswordHash = passwordHasher.HashPassword(password),
                Permissions = permissions
            };

            await adminAccounts.AddAsync(account, cancellationToken);

            logger.LogInformation("Admin account {Username} created.", username);
            console.Success($"admin account '{username}' created.");

            return 0;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to create admin account {Username}.", username);
            console.Error(exception.Message);

            return 1;
        }
    }
}
