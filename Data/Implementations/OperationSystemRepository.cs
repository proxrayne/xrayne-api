using Contracts.Exceptions;
using Data.Contracts;
using Data.Entities;
using Data.Models;
using Data.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Data.Implementations;

/// <summary>
/// Provides EF Core persistence operations for operating systems.
/// </summary>
public sealed class OperationSystemRepository(AppDbContext dbContext) : IOperationSystemRepository
{
    private IQueryable<OperationSystemEntity> OperationSystemsWithRelations => dbContext.OperationSystems
        .Include(operationSystem => operationSystem.Image);

    /// <inheritdoc />
    public Task<List<OperationSystemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return OperationSystemsWithRelations
            .OrderBy(operationSystem => operationSystem.Name)
            .ThenBy(operationSystem => operationSystem.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<OperationSystemEntity?> GetByIdOrDefaultAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return OperationSystemsWithRelations
            .SingleOrDefaultAsync(operationSystem => operationSystem.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OperationSystemEntity> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var operationSystem = await GetByIdOrDefaultAsync(id, cancellationToken);

        return operationSystem ?? throw new NotFoundException($"Operating system '{id}' was not found.");
    }

    /// <inheritdoc />
    public Task<List<OperationSystemEntity>> GetByIdsAsync(
        IReadOnlyCollection<string> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Task.FromResult(new List<OperationSystemEntity>());
        }

        return OperationSystemsWithRelations
            .Where(operationSystem => ids.Contains(operationSystem.Id))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OperationSystemEntity> AddAsync(
        OperationSystemEntity operationSystem,
        CancellationToken cancellationToken = default)
    {
        await dbContext.OperationSystems.AddAsync(operationSystem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdOrDefaultAsync(operationSystem.Id, cancellationToken) ?? operationSystem;
    }

    /// <inheritdoc />
    public async Task<OperationSystemEntity> UpdateAsync(
        string id,
        OperationSystemEntity operationSystem,
        CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(id, ct);

        existing.Name = operationSystem.Name;
        existing.Note = operationSystem.Note;
        existing.Enabled = operationSystem.Enabled;
        existing.Image.Alt = operationSystem.Image.Alt;
        ImagePayloads.ApplyPayload(
            existing.Image,
            NormalizeOptionalImagePayload(operationSystem.Image));
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return existing;
    }

    /// <inheritdoc />
    public async Task<OperationSystemEntity> UpdateAsync(
        string id,
        OperationSystemPatch operationSystem,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        var hasPatch = false;

        if (operationSystem.Name.IsSpecified)
        {
            existing.Name = operationSystem.Name.SpecifiedValue!.Trim();
            hasPatch = true;
        }

        if (operationSystem.Note.IsSpecified)
        {
            existing.Note = operationSystem.Note.SpecifiedValue?.Trim() ?? string.Empty;
            hasPatch = true;
        }

        if (operationSystem.Enabled.IsSpecified)
        {
            existing.Enabled = operationSystem.Enabled.SpecifiedValue;
            hasPatch = true;
        }

        if (!hasPatch)
        {
            return existing;
        }

        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return existing;
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
