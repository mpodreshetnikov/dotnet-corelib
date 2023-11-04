using CoreLib.Utils;
using System.Linq.Expressions;

namespace CoreLib.EntityFramework.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// It's like: () => User?.Address?.City ?? "DefaultCity".
    /// Can be compiled to SQL.
    /// </summary>
    /// <param name="selector">Lambda like: () => User.Address.City</param>
    /// <param name="defaultValue">Default value like: "DefaultCity"</param>
    /// <exception cref="InvalidOperationException"></exception>
    public static IQueryable<TResult> SelectOrDefault<TSource, TResult>(
        this IQueryable<TSource> query, Expression<Func<TSource, TResult>> selector, TResult defaultValue = default!)
    {
        var expression = ExpressionUtils.GetNestedMemberOrDefaultLambda(selector, defaultValue);
        return query.Select(expression);
    }
}
