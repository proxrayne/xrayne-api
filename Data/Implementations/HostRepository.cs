using Contracts.Exceptions;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for subscription hosts.
/// </summary>
public sealed class HostRepository(AppDbContext dbContext) : IHostRepository
{
    private const int PositionStep = 10;

    private IQueryable<HostEntity> HostsWithRelations => dbContext.Hosts
        .Include(host => host.Inbound)
        .ThenInclude(inbound => inbound.Node);

    /// <inheritdoc />
    public Task<List<HostEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return HostsWithRelations
            .OrderBy(host => host.Position)
            .ThenBy(host => host.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<HostEntity?> GetByIdOrDefaultAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return HostsWithRelations
            .SingleOrDefaultAsync(host => host.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HostEntity> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var host = await GetByIdAsync(id, cancellationToken);

        return Required(host, id);
    }

    /// <inheritdoc />
    public async Task<List<InboundEntity>> GetInboundOptionsAsync(
        long adminId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var inbounds = await dbContext.Inbounds
            .Include(inbound => inbound.Node)
            .Where(inbound => inbound.AdminId == adminId)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            inbounds = inbounds
                .Where(inbound =>
                    inbound.Tag.Contains(value, StringComparison.OrdinalIgnoreCase)
                    || inbound.Node.Name.Contains(value, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return inbounds
            .OrderBy(inbound => inbound.Node.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(inbound => inbound.Tag, StringComparer.OrdinalIgnoreCase)
            .ThenBy(inbound => inbound.Id)
            .ToList();
    }

    /// <inheritdoc />
    public Task<bool> InboundExistsAsync(
        long inboundId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Inbounds.AnyAsync(
            inbound => inbound.Id == inboundId,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HostEntity> AddAsync(
        long adminId,
        HostEntity host,
        CancellationToken cancellationToken = default)
    {
        host.AdminId = adminId;
        host.Position = await GetNextPositionAsync(cancellationToken);

        await dbContext.Hosts.AddAsync(host, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(host.Id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<HostEntity?> UpdateAsync(
        long id,
        HostEntity host,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);

        ApplyFullUpdate(existing, host);

        await dbContext.SaveChangesAsync(cancellationToken);

        return existing;
    }

    /// <inheritdoc />
    public async Task<HostEntity?> PatchAsync(
        long id,
        HostPatch patch,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        var hasPatch = ApplyPatch(existing, patch);
        if (!hasPatch)
        {
            return existing;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return existing;
    }

    /// <inheritdoc />
    public async Task<List<HostEntity>> UpdateOrderAsync(
        IReadOnlyList<long> hostIds,
        CancellationToken cancellationToken = default)
    {
        var hosts = await HostsWithRelations.ToListAsync(cancellationToken);

        ValidateOrder(hostIds, hosts);

        var hostsById = hosts.ToDictionary(host => host.Id);
        for (var index = 0; index < hostIds.Count; index++)
        {
            hostsById[hostIds[index]].Position = index * PositionStep;
            hostsById[hostIds[index]].UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetAllAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var host = await GetByIdAsync(id, cancellationToken);

        dbContext.Hosts.Remove(host);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<int> GetNextPositionAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Hosts.MaxAsync(host => host.Position, cancellationToken) + PositionStep;
    }

    private static void ApplyFullUpdate(HostEntity existing, HostEntity host)
    {
        existing.Name = host.Name;
        existing.Address = host.Address;
        existing.CountryAlpha2Code = host.CountryAlpha2Code;
        existing.InboundId = host.InboundId;
        existing.Port = host.Port;
        existing.ServerName = host.ServerName;
        existing.Host = host.Host;
        existing.Path = host.Path;
        existing.Security = host.Security;
        existing.ALPN = host.ALPN;
        existing.Fingerprint = host.Fingerprint;
        existing.FragmentTemplate = host.FragmentTemplate;
        existing.NoiseTemplate = host.NoiseTemplate;
        existing.Enabled = host.Enabled;
        existing.IsMuxEnabled = host.IsMuxEnabled;
        existing.IsUseServerNameAsHost = host.IsUseServerNameAsHost;
        existing.IsRandomUseragent = host.IsRandomUseragent;
        existing.AllowIncrease = host.AllowIncrease;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static HostEntity Required(HostEntity? host, long id)
    {
        return host ?? throw new NotFoundException($"Host '{id}' was not found.");
    }

    private static bool ApplyPatch(HostEntity existing, HostPatch patch)
    {
        var hasPatch = false;

        if (patch.Name.IsSpecified)
        {
            existing.Name = patch.Name.SpecifiedValue!;
            hasPatch = true;
        }

        if (patch.Address.IsSpecified)
        {
            existing.Address = patch.Address.SpecifiedValue!;
            hasPatch = true;
        }

        if (patch.CountryAlpha2Code.IsSpecified)
        {
            existing.CountryAlpha2Code = patch.CountryAlpha2Code.SpecifiedValue!;
            hasPatch = true;
        }

        if (patch.InboundId.IsSpecified)
        {
            existing.InboundId = patch.InboundId.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.Port.IsSpecified)
        {
            existing.Port = patch.Port.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.ServerName.IsSpecified)
        {
            existing.ServerName = patch.ServerName.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.Host.IsSpecified)
        {
            existing.Host = patch.Host.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.Path.IsSpecified)
        {
            existing.Path = patch.Path.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.Security.IsSpecified)
        {
            existing.Security = patch.Security.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.ALPN.IsSpecified)
        {
            existing.ALPN = patch.ALPN.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.Fingerprint.IsSpecified)
        {
            existing.Fingerprint = patch.Fingerprint.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.FragmentTemplate.IsSpecified)
        {
            existing.FragmentTemplate = patch.FragmentTemplate.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.NoiseTemplate.IsSpecified)
        {
            existing.NoiseTemplate = patch.NoiseTemplate.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.Enabled.IsSpecified)
        {
            existing.Enabled = patch.Enabled.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.IsMuxEnabled.IsSpecified)
        {
            existing.IsMuxEnabled = patch.IsMuxEnabled.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.IsUseServerNameAsHost.IsSpecified)
        {
            existing.IsUseServerNameAsHost = patch.IsUseServerNameAsHost.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.IsRandomUseragent.IsSpecified)
        {
            existing.IsRandomUseragent = patch.IsRandomUseragent.SpecifiedValue;
            hasPatch = true;
        }

        if (patch.AllowIncrease.IsSpecified)
        {
            existing.AllowIncrease = patch.AllowIncrease.SpecifiedValue;
            hasPatch = true;
        }

        return hasPatch;
    }

    private static void ValidateOrder(
        IReadOnlyList<long> hostIds,
        IReadOnlyCollection<HostEntity> hosts)
    {
        var requested = hostIds.ToHashSet();
        if (requested.Count != hostIds.Count)
        {
            throw new BadRequestException("Host order contains duplicate host ids.");
        }

        var existing = hosts.Select(host => host.Id).ToHashSet();
        if (!requested.SetEquals(existing))
        {
            throw new BadRequestException("Host order must contain every host id exactly once.");
        }
    }
}
