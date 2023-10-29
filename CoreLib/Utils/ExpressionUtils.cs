using System.Linq.Expressions;

namespace CoreLib.Utils;

public class ExpressionUtils
{
    /// <summary>
    /// Returns expression (for IQueriable for example) that allows to get member you need or default value if any of submember on the path will be null.
    /// It's like: () => User?.Address?.City ?? "DefaultCity".
    /// </summary>
    /// <param name="memberAccessExpression">Lambda like: () => User.Address.City</param>
    /// <param name="defaultValue">Default value like: "DefaultCity"</param>
    /// <param name="newParameterExpression">???</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static Expression GetNestedMemberOrDefault<TOut>
            (LambdaExpression memberAccessExpression, TOut defaultValue, ParameterExpression newParameterExpression = default!)
    {
        ArgumentUtils.MustBeNotNull(memberAccessExpression, nameof(memberAccessExpression));

        if (memberAccessExpression.Body is not MemberExpression castedMemberAccessExpression)
        {
            throw new InvalidOperationException("Provided member access chain is invalid.");
        }

        var memberAccessors = new List<MemberExpression>();
        Expression? expression = castedMemberAccessExpression;
        while (expression is MemberExpression castedExpression)
        {
            memberAccessors.Add(castedExpression);
            expression = castedExpression.Expression;
        }
        memberAccessors.Reverse();

        var rootExpression = newParameterExpression
            ?? (expression is ParameterExpression
                ? expression
                : throw new InvalidOperationException("Provided member access chain is invalid."));

        return MakeConditionalExpression(rootExpression, defaultValue, memberAccessors);
    }

    public static Expression MakeConditionalExpression<TOut>(Expression expression, TOut defaultValue, IEnumerable<MemberExpression> memberExpressions)
    {
        if (!memberExpressions.Any())
        {
            return expression;
        }

        var testIsNullExpression = Expression.Equal(expression, Expression.Constant(null));
        var ifTrueExpression = Expression.Constant(defaultValue);
        var ifFalseExpression = MakeConditionalExpression(
            Expression.MakeMemberAccess(expression, memberExpressions.First().Member),
            defaultValue,
            memberExpressions.Skip(1));
        return Expression.Condition(testIsNullExpression, ifTrueExpression, ifFalseExpression);
    }
}
