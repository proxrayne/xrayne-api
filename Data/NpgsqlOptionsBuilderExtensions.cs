using Contracts.Enums;
using Contracts.Utilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Xray.Config.Enums;

namespace Data;

/// <summary>
/// Provides shared PostgreSQL provider configuration for XRayne database contexts.
/// </summary>
public static class NpgsqlOptionsBuilderExtensions
{
    private static readonly NpgsqlEnumMapping[] EnumMappings =
    [
        CreateEnumMapping<UserStatus>("user_status"),
        CreateEnumMapping<LimitResetStrategy>("limit_reset_strategy"),
        CreateEnumMapping<SSHAuthType>("ssh_auth_type"),
        CreateEnumMapping<CertificateMode>("certificate_mode"),
        CreateEnumMapping<GeoResourceStatus>("geo_resource_status"),
        CreateEnumMapping<XtlsFlow>("xtls_flow"),
        CreateEnumMapping<EncryptionMethod>("encryption_method"),
        CreateEnumMapping<SubscriptionFormat>("subscription_format")
    ];

    /// <summary>
    /// Configures PostgreSQL enum type declarations used by EF migrations.
    /// </summary>
    public static ModelBuilder ConfigureXRaynePostgresEnums(this ModelBuilder builder)
    {
        foreach (var mapping in EnumMappings)
        {
            mapping.ConfigureModel(builder);
        }

        return builder;
    }

    /// <summary>
    /// Configures the DbContext options builder with XRayne PostgreSQL mappings.
    /// </summary>
    public static DbContextOptionsBuilder UseXRayneNpgsql(
        this DbContextOptionsBuilder builder,
        string connectionString)
    {
        return builder.UseNpgsql(
            connectionString,
            options => options.ConfigureMappings());
    }

    /// <summary>
    /// Configures the typed DbContext options builder with XRayne PostgreSQL mappings.
    /// </summary>
    public static DbContextOptionsBuilder<TContext> UseXRayneNpgsql<TContext>(
        this DbContextOptionsBuilder<TContext> builder,
        string connectionString)
        where TContext : DbContext
    {
        return builder.UseNpgsql(
            connectionString,
            options => options.ConfigureMappings());
    }

    /// <summary>
    /// Creates an Npgsql data source with the same enum and JSON mappings used by EF Core.
    /// </summary>
    public static NpgsqlDataSource CreateXRayneDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        
        ConfigureDataSource(builder);

        return builder.Build();
    }

    private static void ConfigureMappings(this NpgsqlDbContextOptionsBuilder builder)
    {
        foreach (var mapping in EnumMappings)
        {
            mapping.ConfigureOptions(builder);
        }

        builder.ConfigureDataSource(ConfigureDataSource);
    }

    private static void ConfigureDataSource(NpgsqlDataSourceBuilder builder)
    {
        foreach (var mapping in EnumMappings)
        {
            mapping.ConfigureDataSource(builder);
        }

        builder
            .EnableDynamicJson()
            .ConfigureJsonOptions(XrayJsonSerializer.Options);
    }

    private static NpgsqlEnumMapping CreateEnumMapping<TEnum>(
        string databaseTypeName,
        bool configureRuntimeMapping = true)
        where TEnum : struct, Enum
    {
        static void NoRuntimeMapping<TBuilder>(TBuilder _) { }

        return new NpgsqlEnumMapping(
            model => model.HasPostgresEnum<TEnum>(name: databaseTypeName),
            configureRuntimeMapping
                ? options => options.MapEnum<TEnum>(databaseTypeName)
                : NoRuntimeMapping<NpgsqlDbContextOptionsBuilder>,
            configureRuntimeMapping
                ? dataSource => dataSource.MapEnum<TEnum>(databaseTypeName)
                : NoRuntimeMapping<NpgsqlDataSourceBuilder>);
    }

    private sealed record NpgsqlEnumMapping(
        Action<ModelBuilder> ConfigureModel,
        Action<NpgsqlDbContextOptionsBuilder> ConfigureOptions,
        Action<NpgsqlDataSourceBuilder> ConfigureDataSource);
}
