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
    /// <param name="newParameterExpression">Expression to be used as a root for the member accessors chain.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static Expression<Func<TIn, TOut>> GetNestedMemberOrDefaultLambda<TIn, TOut>
            (Expression<Func<TIn, TOut>> memberAccessExpression,
            TOut defaultValue,
            ParameterExpression newParameterExpression = default!)
    {
        var (expression, rootExpression) = GetNestedMemberOrDefaultExpression(memberAccessExpression, defaultValue, newParameterExpression);
        return Expression.Lambda<Func<TIn, TOut>>(expression, rootExpression);
    }

    /// <summary>
    /// Returns (resultExpression, rootExpression) that allows to get member you need or default value if any of submember on the path will be null.
    /// </summary>
    internal static (Expression, ParameterExpression) GetNestedMemberOrDefaultExpression<TIn, TOut>
            (Expression<Func<TIn, TOut>> memberAccessExpression,
            TOut defaultValue,
            ParameterExpression newParameterExpression = default!)
    {
        ArgumentNullException.ThrowIfNull(memberAccessExpression, nameof(memberAccessExpression));

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

        ParameterExpression rootExpression = newParameterExpression
            ?? (expression is ParameterExpression parameterExpression
                ? parameterExpression
                : throw new InvalidOperationException("Provided member access chain is invalid."));

        return (MakeConditionalExpression(rootExpression, defaultValue, memberAccessors), rootExpression);
    }

    private static Expression MakeConditionalExpression<TOut>(
        Expression expression, TOut defaultValue, IEnumerable<MemberExpression> memberExpressions)
    {
        if (!memberExpressions.Any())
        {
            return expression;
        }

        var testIsNullExpression = Expression.Equal(expression, Expression.Constant(null!, expression.Type));
        var ifTrueExpression = Expression.Constant(defaultValue, typeof(TOut));
        var ifFalseExpression = MakeConditionalExpression(
            Expression.MakeMemberAccess(expression, memberExpressions.First().Member),
            defaultValue,
            memberExpressions.Skip(1));
        return Expression.Condition(testIsNullExpression, ifTrueExpression, ifFalseExpression);
    }
}
