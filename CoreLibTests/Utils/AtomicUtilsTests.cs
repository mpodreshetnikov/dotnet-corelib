using CoreLib.Utils;
using FluentAssertions;

namespace EfCoreExtensionTests.Utils;

public class AtomicUtilsTests
{
    [Fact]
    public void SafelyJoin_JoinWithEmptyOne_AllExludeEmptyOne()
    {
        // Arrange
        var strings = new[] { "A", "", "C" };
        const string separator = ":";
        const string expected = "A:C";
        // Action
        var actual = AtomicUtils.SafelyJoin(separator, strings);
        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void SafelyJoin_JoinWithEmptyOne_InitialValuesAreNotCorrupted()
    {
        // Arrange
        var strings = new[] { "A", "", "C" };
        const string separator = ":";
        var expected = new[] { "A", "", "C" };
        // Action
        _ = AtomicUtils.SafelyJoin(separator, strings);
        // Assert
        strings.Should().BeEquivalentTo(expected);
    }
}