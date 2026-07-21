using Contracts.Exceptions;
using Contracts.Values;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Data.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for client applications.
/// </summary>
public sealed class ApplicationRepository(AppDbContext context) : IApplicationRepository
{
    private IQueryable<ApplicationEntity> ApplicationsWithRelations => context.Applications
        .Include(application => application.Image)
        .Include(application => application.OperationSystems)
        .ThenInclude(operationSystem => operationSystem.Image);

    /// <inheritdoc />
    public Task<List<ApplicationEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return ApplicationsWithRelations
            .OrderBy(application => application.Name)
            .ThenBy(application => application.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<ApplicationEntity?> GetByIdOrDefaultAsync(int id, CancellationToken cancellationToken = default)
    {
        return ApplicationsWithRelations
            .SingleOrDefaultAsync(application => application.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApplicationEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var application = await GetByIdOrDefaultAsync(id, cancellationToken);

        return application ?? throw new NotFoundException($"Application '{id}' was not found.");
    }

    /// <inheritdoc />
    public async Task<ApplicationEntity> AddAsync(
        ApplicationEntity application,
        CancellationToken ct = default)
    {
        await context.Applications.AddAsync(application, ct);
        await context.SaveChangesAsync(ct);
        await context.Entry(application).ReloadAsync(ct);

        return application;
    }

    /// <inheritdoc />
    public async Task<ApplicationEntity> UpdateAsync(
        int id,
        ApplicationPatch application,
        CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);

        var hasPatch = false;

        if (application.Name.IsSpecified)
        {
            existing.Name = application.Name.SpecifiedValue!.Trim();
            hasPatch = true;
        }

        if (application.WebsiteUrl.IsSpecified)
        {
            existing.WebsiteUrl = application.WebsiteUrl.SpecifiedValue?.Trim();
            hasPatch = true;
        }

        if (application.Protocol.IsSpecified)
        {
            existing.Protocol = application.Protocol.SpecifiedValue?.Trim();
            hasPatch = true;
        }

        if (application.DetectPattern.IsSpecified)
        {
            existing.DetectPattern = application.DetectPattern.SpecifiedValue!.Trim();
            hasPatch = true;
        }

        if (application.SubscriptionFormat.IsSpecified)
        {
            existing.SubscriptionFormat = application.SubscriptionFormat.SpecifiedValue;
            hasPatch = true;
        }

        if (application.Enabled.IsSpecified)
        {
            existing.Enabled = application.Enabled.SpecifiedValue;
            hasPatch = true;
        }

        if (application.Assets.IsSpecified)
        {
            existing.Assets = application.Assets.SpecifiedValue?.ToList()
                ?? throw new BadRequestException("Application assets are required.");
            hasPatch = true;
        }

        if (application.OperationSystemIds.IsSpecified)
        {
            var operationSystems = await ResolveOperationSystemsAsync(
                application.OperationSystemIds.SpecifiedValue
                    ?? throw new BadRequestException("Operating system identifiers are required."),
                ct);

            existing.OperationSystems.Clear();
            foreach (var operationSystem in operationSystems)
            {
                existing.OperationSystems.Add(operationSystem);
            }

            hasPatch = true;
        }

        if (!hasPatch)
        {
            return existing;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(ct);

        return existing;
    }


    /// <inheritdoc />
    public async Task<ApplicationEntity> UpdateAsync(
        int id,
        ApplicationEntity application,
        CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);

        existing.Name = application.Name;
        existing.WebsiteUrl = application.WebsiteUrl;
        existing.Protocol = application.Protocol;
        existing.DetectPattern = application.DetectPattern;
        existing.SubscriptionFormat = application.SubscriptionFormat;
        existing.Enabled = application.Enabled;
        existing.Assets = application.Assets.ToList();
        existing.OperationSystems.Clear();

        existing.Image.Alt = application.Image.Alt;
        ImagePayloads.ApplyPayload(
            existing.Image,
            NormalizeOptionalImagePayload(application.Image));

        foreach (var operationSystem in application.OperationSystems)
        {
            existing.OperationSystems.Add(operationSystem);
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(ct);

        return existing;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var application = await GetByIdAsync(id, cancellationToken);

        var image = application.Image;

        context.Applications.Remove(application);
        context.Images.Remove(image);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<OperationSystemEntity>> ResolveOperationSystemsAsync(
        IReadOnlyCollection<string> operationSystemIds,
        CancellationToken cancellationToken)
    {
        if (operationSystemIds.Count == 0)
        {
            throw new BadRequestException("At least one operating system is required.");
        }

        var resolved = await context.OperationSystems
            .Include(operationSystem => operationSystem.Image)
            .Where(operationSystem => operationSystemIds.Contains(operationSystem.Id))
            .ToListAsync(cancellationToken);

        if (resolved.Count != operationSystemIds.Count)
        {
            throw new BadRequestException("One or more selected operating systems were not found.");
        }

        return resolved;
    }

    private static ImagePayload? NormalizeOptionalImagePayload(ImageEntity incoming)
    {
        var hasContent = incoming.Content.Length > 0;
        var hasContentType = !string.IsNullOrWhiteSpace(incoming.ContentType);

        if (!hasContent)
        {
            return null;
        }

        if (!hasContentType)
        {
            throw new BadRequestException("Image content type is required.");
        }

        return new ImagePayload(incoming.Content, incoming.ContentType);
    }
}
