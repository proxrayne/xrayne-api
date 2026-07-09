using Contracts.Models;

namespace Test.Data;

/// <summary>
/// Tests user query contracts.
/// </summary>
public sealed class UserRepositoryTests
{
    [Fact]
    public void UserFilter_DoesNotExposeProtocol()
    {
        typeof(UserFilter).GetProperty("Protocol").Should().BeNull();
    }
}
