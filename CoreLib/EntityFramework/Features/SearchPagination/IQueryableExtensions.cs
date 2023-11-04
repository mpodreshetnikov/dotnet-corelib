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
            this IQueryable<T> query, IPagedQuery pagedQuery)
    {
        if (pagedQuery is null)
        {
            return query;
        }

        if (pagedQuery.Offset.HasValue)
        {
            query = query.Skip(pagedQuery.Offset.Value);
        }
        if (pagedQuery.Limit.HasValue)
        {
            query = query.Take(pagedQuery.Limit.Value);
        }

        return query;
    }

    /// <summary>
    /// Add pagination to the query and enumerate it.<br></br>
    /// CAUTION: Set <paramref name="applyPagination"/> to false if used <see cref="ApplyPagination{T}(IQueryable{T}, IPagedQuery)"/>, otherwise it will double the pagination.
    /// </summary>
    /// <param name="applyPagination">If true – use <see cref="ApplyPagination{T}(IQueryable{T}, IPagedQuery)"/> to the query.</param>
    /// <param name="totalItemsRewrite">
    /// Value to be set as "TotalItems" in <see cref="PagedResult{T}"/>.
    /// Useful if query was already paginated to set actual total items quantity.
    /// Required!!! if <paramref name="applyPagination"/> is false.
    /// </param>
    /// <returns>Paged result</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<PagedResult<T>> AsPagedResultAsync<T>(
        this IQueryable<T> query,
        IPagedQuery pagedQuery,
        CancellationToken cancellationToken = default,
        bool applyPagination = true,
        long? totalItemsRewrite = null!)
    {
        if (pagedQuery is null)
        {
            throw new ArgumentNullException(nameof(pagedQuery));
        }

        if (applyPagination == false
            && totalItemsRewrite is null)
        {
            throw new ArgumentNullException(nameof(totalItemsRewrite), "Required if 'applyPagination' is false");
        }

        var paginatedQueryable = query;
        if (applyPagination)
        {
            paginatedQueryable = query.ApplyPagination(pagedQuery);
        }

        return new PagedResult<T>()
        {
            Items = await paginatedQueryable.ToListAsync(cancellationToken),
            ItemsOffset = pagedQuery.Offset ?? 0,
            ItemsQuantity = await paginatedQueryable.CountAsync(cancellationToken),
            TotalItems = totalItemsRewrite ?? await query.CountAsync(cancellationToken),
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
        propsToSearch.MustContainsAtLeast(1, nameof(propsToSearch));
        propsToSearch.AllElementsMustBeNotNull(nameof(propsToSearch));

        if (ReflectionUtils.IsQueryOrdered(query) == false)
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
