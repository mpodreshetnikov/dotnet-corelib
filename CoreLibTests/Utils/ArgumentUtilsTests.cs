using CoreLib.Utils;

namespace EfCoreExtensionTests.Utils;

public class ArgumentUtilsTests
{
    #region DefaultIfNull

    [Fact]
    public void DefaultIfNull_ReferenceArgumentNotNull_ArgumentNotChanged()
    {
        // Arrange
        var argument = new Faker().Person;
        var linkToArgument = argument;
        // Action
        var actual = ArgumentUtils.DefaultIfNull(argument);
        // Assert
        actual.Should().Be(argument);
        linkToArgument.Should().Be(argument);
    }

    [Fact]
    public void DefaultIfNull_ValueArgumentNotNull_ArgumentNotChanged()
    {
        // Arrange
        var argument = new Faker().Random.Int();
        // Action
        var actual = ArgumentUtils.DefaultIfNull(argument);
        // Assert
        actual.Should().Be(argument);
    }

    [Fact]
    public void DefaultIfNull_ReferenceArgumentIsNull_ReturnedChangedToProvided()
    {
        // Arrange
        Person? argument = null;
        var newArgument = new Faker().Person;
        // Action
        var actual = ArgumentUtils.DefaultIfNull(argument, newArgument);
        // Assert
        actual.Should().Be(newArgument);
    }

    [Fact]
    public void DefaultIfNull_ValueArgumentIsNull_ReturnedChangedToProvided()
    {
        // Arrange
        int? argument = null;
        var newArgument = new Faker().Random.Int();
        // Action
        var actual = ArgumentUtils.DefaultIfNull(argument, newArgument);
        // Assert
        actual.Should().Be(newArgument);
    }

    #endregion

    #region MustBeNotNull

    [Fact]
    public void MustBeNotNull_ArgumentNotNull_NotThrows()
    {
        // Arrange
        var argument = new Faker().Person;
        var paramName = new Faker().Random.String();
        // Assert
        FluentActions.Invoking(() => ArgumentUtils.MustBeNotNull(argument, paramName))
            .Should().NotThrow();
    }

    [Fact]
    public void MustBeNotNull_ArgumentIsNull_Throws()
    {
        // Arrange
        Person? argument = null;
        var paramName = new Faker().Random.String();
        // Assert
        FluentActions.Invoking(() => ArgumentUtils.MustBeNotNull(argument, paramName))
            .Should().Throw<ArgumentNullException>()
                .And.ParamName.Should().Be(paramName);
    }

    #endregion
}