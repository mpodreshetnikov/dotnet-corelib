using Bogus;
using System.Linq.Expressions;
using CoreLib.Utils;
using FluentAssertions;
using CoreLib.EntityFramework.Extensions;

namespace EfCoreExtensionTests.Utils;

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
        var query = new Faker().MakeLazy(10, () => new Person()).AsQueryable();
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