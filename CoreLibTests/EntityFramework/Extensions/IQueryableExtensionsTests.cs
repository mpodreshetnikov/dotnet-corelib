using Bogus;
using CoreLib.EntityFramework.Extensions;
using FluentAssertions;

namespace EfCoreExtensionTests.Utils.EntityFramework.Extensions;

public class IQueryableExtensionsTests
{
    #region SelectOrDefault

    [Fact]
    public void SelectOrDefault_NotMemberAccessorExpression_ThrowsException()
    {
        // Arrange
        var query = new Faker().MakeLazy(10, () => new Person()).AsQueryable();
        // Assert
        FluentActions.Invoking(() =>
            query.SelectOrDefault(person => person.FirstName + person.LastName))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SelectOrDefault_AccessQueryWithNullMember_ReturnsDefaultValueWhereNull()
    {
        // Arrange
        // 3 persons
        var query = new Faker().Make(3, () => new Person()).AsQueryable();
        query.ElementAt(1).Company = null;

        // default value where company is null
        var expected = new string[] { query.ElementAt(0).Company.Name, default!, query.ElementAt(2).Company.Name };

        // Action
        var actual = query.SelectOrDefault(person => person.Company.Name);

        // Assert
        actual.Should().Equal(expected);
    }

    [Fact]
    public void SelectOrDefault_AccessQueryWithNullMemberAndSetDefaultValueManual_ReturnsDefaultValueWhereNull()
    {
        // Arrange
        // 3 persons
        var query = new Faker().Make(3, () => new Person()).AsQueryable();
        query.ElementAt(1).Company = null;
        var _default = "default";

        // default value where company is null
        var expected = new string[] { query.ElementAt(0).Company.Name, _default, query.ElementAt(2).Company.Name };

        // Action
        var actual = query.SelectOrDefault(person => person.Company.Name, _default);

        // Assert
        actual.Should().Equal(expected);
    }

    #endregion
}