using Contracts.Models;
using Data.Entities;

namespace Data.Contracts;

public interface IUserRepository
{
    Task<List<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<UserEntity>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<CursorPage<UserEntity>> SearchAsync(UserFilter filter, CancellationToken cancellationToken = default);

    Task<CursorPage<UserEntity>> SearchAsync(Guid adminId, UserFilter filter, CancellationToken cancellationToken = default);

    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserEntity?> GetByIdAsync(Guid adminId, Guid id, CancellationToken cancellationToken = default);

    Task<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<UserEntity?> GetByUsernameAsync(Guid adminId, string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid adminId, string username, CancellationToken cancellationToken = default);

    Task<UserEntity> AddAsync(UserEntity user, CancellationToken cancellationToken = default);

    Task<UserEntity?> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default);

    Task<UserEntity?> UpdateAsync(Guid adminId, UserEntity user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, Guid id, CancellationToken cancellationToken = default);
}
