using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test.Utilities;

/// <summary>
/// Creates AutoMapper test configuration with the logger factory required by current AutoMapper versions.
/// </summary>
public static class MapperTestFactory
{
    /// <summary>
    /// Creates an AutoMapper configuration for tests.
    /// </summary>
    public static MapperConfiguration CreateConfiguration(Action<IMapperConfigurationExpression> configure)
    {
        return new MapperConfiguration(configure, NullLoggerFactory.Instance);
    }
}
