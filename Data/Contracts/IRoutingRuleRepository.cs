using Data.Entities;

namespace Data.Contracts;

/// <summary>
/// Stores routing rule entities.
/// </summary>
public interface IRoutingRuleRepository
{
    /// <summary>
    /// Gets all routing rules.
    /// </summary>
    Task<List<RoutingRuleEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all routing rules owned by an administrator.
    /// </summary>
    Task<List<RoutingRuleEntity>> GetAllAsync(long adminId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets routing rules assigned to a remote node.
    /// </summary>
    Task<List<RoutingRuleEntity>> GetByNodeIdAsync(long nodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a routing rule by id.
    /// </summary>
    Task<RoutingRuleEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an administrator-owned routing rule by id.
    /// </summary>
    Task<RoutingRuleEntity?> GetByIdAsync(long adminId, long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one routing rule assigned to a remote node.
    /// </summary>
    Task<RoutingRuleEntity?> GetByNodeAndIdAsync(long nodeId, long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one routing rule assigned to a remote node by ruleTag.
    /// </summary>
    Task<RoutingRuleEntity?> GetByNodeAndRuleTagAsync(
        long nodeId,
        string ruleTag,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a routing rule.
    /// </summary>
    Task<RoutingRuleEntity> AddAsync(RoutingRuleEntity routingRule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a routing rule assigned to an administrator and remote node.
    /// </summary>
    Task<RoutingRuleEntity> AddAsync(
        long adminId,
        long nodeId,
        RoutingRuleEntity routingRule,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a routing rule.
    /// </summary>
    Task<RoutingRuleEntity?> UpdateAsync(RoutingRuleEntity routingRule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an administrator-owned routing rule.
    /// </summary>
    Task<RoutingRuleEntity?> UpdateAsync(long adminId, RoutingRuleEntity routingRule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves routing rule creations, updates, and deletions in one database operation.
    /// </summary>
    Task<List<RoutingRuleEntity>> SaveChangesAsync(
        long adminId,
        long nodeId,
        IReadOnlyCollection<RoutingRuleEntity> rulesToCreate,
        IReadOnlyCollection<RoutingRuleEntity> rulesToUpdate,
        IReadOnlyCollection<RoutingRuleEntity> rulesToDelete,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a routing rule by id.
    /// </summary>
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an administrator-owned routing rule by id.
    /// </summary>
    Task<bool> DeleteAsync(long adminId, long id, CancellationToken cancellationToken = default);
}
