using System.Data.Common;
using Npgsql;
using XRayne.Repositories.Abstractions;

namespace XRayne.Repositories.PostgreSql;

public sealed class PostgreSqlConnectionFactory : IPostgreSqlConnectionFactory
{
    private readonly NpgsqlDataSource dataSource;

    public PostgreSqlConnectionFactory(NpgsqlDataSource dataSource)
    {
        this.dataSource = dataSource;
    }

    public async ValueTask<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await dataSource.OpenConnectionAsync(cancellationToken);
    }
}
