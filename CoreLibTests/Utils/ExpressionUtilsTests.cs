using System.Linq.Expressions;
using CoreLib.Utils;

namespace EfCoreExtensionTests.Utils;

public class ExpressionUtilsTests
{
    #region GetNestedMemberOrDefault

    [Fact]
    public void GetNestedMemberOrDefault_NotMemberAccessorExpression_ThrowsException()
    {
        // Arrange
        Expression<Func<Person, string>> expression = (person) => person.FirstName + person.LastName;
        // Assert
        FluentActions.Invoking(() => ExpressionUtils.GetNestedMemberOrDefaultLambda(expression, string.Empty))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetNestedMemberOrDefault_AccessNullValueMember_ReturnsDefault()
    {
        // Arrange
        var person = new Faker().Person;
        person.Company = null;
        var _default = "default";

        Expression<Func<Person, string>> expression = (person) => person.Company.Name;
        var expressionToTest = ExpressionUtils.GetNestedMemberOrDefaultLambda(expression, _default);
        // Action
        var actual = expressionToTest.Compile().Invoke(person);
        // Assert
        actual.Should().Be(_default);
    }

    [Fact]
    public void GetNestedMemberOrDefault_AccessNotNullValueMember_ReturnsValueMember()
    {
        // Arrange
        var person = new Faker().Person;
        var _default = "default";

        Expression<Func<Person, string>> expression = (person) => person.Company.Name;
        var expressionToTest = ExpressionUtils.GetNestedMemberOrDefaultLambda(expression, _default);
        // Action
        var actual = expressionToTest.Compile().Invoke(person);
        // Assert
        actual.Should().Be(person.Company.Name);
    }

    #endregion
}