using Contracts.Models;
using Data.Entities;

namespace Data.Contracts;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<User>> GetAllAsync(Guid adminId, CancellationToken cancellationToken = default);

    Task<CursorPage<User>> SearchAsync(UserFilter filter, CancellationToken cancellationToken = default);

    Task<CursorPage<User>> SearchAsync(Guid adminId, UserFilter filter, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid adminId, Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<User?> GetByUsernameAsync(Guid adminId, string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid adminId, string username, CancellationToken cancellationToken = default);

    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> UpdateAsync(User user, CancellationToken cancellationToken = default);

    Task<User?> UpdateAsync(Guid adminId, User user, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid adminId, Guid id, CancellationToken cancellationToken = default);
}
