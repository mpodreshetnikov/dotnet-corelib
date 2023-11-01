using System.Linq.Expressions;
using Bogus;
using CoreLib.Utils;
using FluentAssertions;

namespace EfCoreExtensionTests.Utils;

public class ExpressionUtilsTests
{
    public Faker Faker { get; set; } = new Faker();

    #region GetNestedMemberOrDefault

    [Fact]
    public void GetNestedMemberOrDefault_NotMemberAccessorExpression_ThrowsException()
    {
        // Arrange
        Expression<Func<Person, string>> expression = (person) => person.FirstName + person.LastName;
        // Assert
        FluentActions.Invoking(() => ExpressionUtils.GetNestedMemberOrDefault(expression, string.Empty))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetNestedMemberOrDefault_AccessNullValueMember_ReturnsDefault()
    {
        // Arrange
        var person = Faker.Person;
        person.Company = null;
        var _default = "default";

        Expression<Func<Person, string>> expression = (person) => person.Company.Name;
        var expressionToTest = ExpressionUtils.GetNestedMemberOrDefault(expression, _default);
        // Action
        var actual = expressionToTest.Compile().Invoke(person);
        // Assert
        actual.Should().Be(_default);
    }

    [Fact]
    public void GetNestedMemberOrDefault_AccessNotNullValueMember_ReturnsValueMember()
    {
        // Arrange
        var person = Faker.Person;
        var _default = "default";

        Expression<Func<Person, string>> expression = (person) => person.Company.Name;
        var expressionToTest = ExpressionUtils.GetNestedMemberOrDefault(expression, _default);
        // Action
        var actual = expressionToTest.Compile().Invoke(person);
        // Assert
        actual.Should().Be(person.Company.Name);
    }

    #endregion
}