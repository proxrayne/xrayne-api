using System.Data.Common;

namespace XRayne.Repositories.Abstractions;

public interface IPostgreSqlConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
