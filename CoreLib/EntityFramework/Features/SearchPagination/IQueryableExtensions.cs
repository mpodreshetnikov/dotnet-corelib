using CoreLib.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CoreLib.EntityFramework.Features.SearchPagination;

public static class IQueryableExtensions
{
    /// <summary>
    /// Add pagination to query.
    /// </summary>
    /// <returns>Query.</returns>
    public static IQueryable<T> ApplyPagination<T>(
            this IQueryable<T> query, IPagedQuery paginatedQuery)
    {
        if (paginatedQuery is null)
        {
            return query;
        }

        if (paginatedQuery.Offset.HasValue)
        {
            query = query.Skip(paginatedQuery.Offset.Value);
        }
        if (paginatedQuery.Limit.HasValue)
        {
            query = query.Take(paginatedQuery.Limit.Value);
        }

        return query;
    }

    /// <summary>
    /// Add pagination to the query and enumerates it.<br></br>
    /// CAUTION: Don't use it with <see cref="ApplyPagination{T}(IQueryable{T}, IPagedQuery)"/>, it will double pagination.
    /// </summary>
    /// <returns>Paged result</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<PagedResult<T>> AsPagedResultAsync<T>(
        this IQueryable<T> query,
        IPagedQuery paginatedQuery,
        CancellationToken cancellationToken = default)
    {
        if (paginatedQuery is null)
        {
            throw new ArgumentNullException(nameof(paginatedQuery));
        }

        var paginatedQueryable = query.ApplyPagination(paginatedQuery);

        return new PagedResult<T>()
        {
            Items = await paginatedQueryable.ToListAsync(cancellationToken),
            ItemsOffset = paginatedQuery.Offset ?? 0,
            ItemsQuantity = await paginatedQueryable.CountAsync(cancellationToken),
            TotalItems = await query.CountAsync(cancellationToken),
        };
    }

    /// <summary>
    /// Filter query by search string.<br></br>
    /// You can provide multiple properties to search. Search string will be concatenated from them in the provided order.<br></br>
    /// Works only with string properties. Can be compiled to SQL.<br></br>
    /// Case insensitive.<br></br>
    /// Query must be ordered before using this method.
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
            this IQueryable<T> query, ISearchQuery searchQuery,
            params Expression<Func<T, string?>>[] propsToSearch)
    {
        ArgumentUtils.AllMustBeNotNull(propsToSearch, nameof(propsToSearch));

        if (OrderingReflectionUtils.IsQueryOrdered(query) == false)
        {
            throw new ArgumentException("Query must be ordered before using this method.", nameof(query));
        }

        if (searchQuery is null || string.IsNullOrEmpty(searchQuery.SearchQuery))
        {
            return query;
        }

        // Create lambda for Where clause.
        var parameter = Expression.Parameter(typeof(T));

        // Build search string.
        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

        var expressionsToConcat = new List<Expression>();
        foreach (var prop in propsToSearch)
        {
            var safeMemberAccessExpression = ExpressionUtils.GetNestedMemberOrDefault(prop, string.Empty, parameter);
            expressionsToConcat.Add(Expression.Add(safeMemberAccessExpression, Expression.Constant(" "), concatMethodInfo));
        }
        var searchStringExpression = expressionsToConcat.Aggregate((leftExpr, rightExpr) => Expression.Add(leftExpr, rightExpr, concatMethodInfo));
        searchStringExpression = Expression.Call(searchStringExpression, nameof(string.ToLower), null);

        // Check if it contains searching query string.
        var searchingQueryExpression = Expression.Constant(searchQuery.SearchQuery.Trim().ToLower());
        var containsCallExpression = Expression.Call(searchStringExpression, nameof(string.Contains), null, searchingQueryExpression);

        // Create Where clause lambda.
        var whereLambda = Expression.Lambda(containsCallExpression, parameter);

        // Apply search
        query = query.Where((Expression<Func<T, bool>>)whereLambda);

        return query;
    }
}
