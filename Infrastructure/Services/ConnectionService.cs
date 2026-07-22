using Contracts.Enums;
using Contracts.Exceptions;
using Contracts.Models;
using Contracts.Utilities;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Xray.Config.Enums;

namespace Infrastructure.Services;

/// <summary>
/// Provides application behavior for user connections.
/// </summary>
public sealed class ConnectionService(
    IConnectionRepository connections,
    IUserRepository users) : IConnectionService
{
    /// <inheritdoc />
    public async Task<OffsetPage<ConnectionEntity>> GetByUserIdAsync(
        long userId,
        ConnectionFilter filter,
        CancellationToken cancellationToken = default)
    {
        await users.GetByIdAsync(userId, cancellationToken);

        return await connections.SearchByUserIdAsync(userId, filter, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ConnectionEntity> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return connections.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConnectionEntity> CreateAsync(
        long userId,
        string name,
        XtlsFlow flow,
        EncryptionMethod method,
        DeviceVerificationMethod deviceVerificationMethod,
        CancellationToken cancellationToken = default)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if ((uint)user.Connections.Count(connection => !connection.Revoked) >= user.ConnectionLimit)
        {
            throw new ForbiddenException("Connection limit is exhausted.");
        }

        var connection = new ConnectionEntity
        {
            UserId = user.Id,
            Name = NormalizeName(name),
            Uuid = Guid.NewGuid(),
            Password = XraySecretGenerator.GeneratePassword(),
            Flow = flow,
            Method = method,
            DeviceVerificationMethod = deviceVerificationMethod
        };

        return await connections.AddAsync(connection, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConnectionEntity> UpdateAsync(
        long id,
        string name,
        XtlsFlow flow,
        EncryptionMethod method,
        DeviceVerificationMethod deviceVerificationMethod,
        CancellationToken cancellationToken = default)
    {
        var connection = await connections.GetByIdAsync(id, cancellationToken);

        connection.Name = NormalizeName(name);
        connection.Flow = flow;
        connection.Method = method;
        connection.DeviceVerificationMethod = deviceVerificationMethod;

        return await connections.UpdateAsync(connection, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ConnectionEntity> PatchAsync(
        long id,
        ConnectionPatch patch,
        CancellationToken cancellationToken = default)
    {
        var connection = await connections.GetByIdAsync(id, cancellationToken);

        if (patch.Name.IsSpecified)
        {
            connection.Name = NormalizeName(patch.Name.SpecifiedValue);
        }

        if (patch.Flow.IsSpecified)
        {
            connection.Flow = patch.Flow.SpecifiedValue;
        }

        if (patch.Method.IsSpecified)
        {
            connection.Method = patch.Method.SpecifiedValue;
        }

        if (patch.DeviceVerificationMethod.IsSpecified)
        {
            connection.DeviceVerificationMethod = patch.DeviceVerificationMethod.SpecifiedValue;
        }

        return await connections.UpdateAsync(connection, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ConnectionEntity> RevokeByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return connections.RevokeByIdAsync(id, cancellationToken);
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Connection name is required.");
        }

        return name.Trim();
    }
}
