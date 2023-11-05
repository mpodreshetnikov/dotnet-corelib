using Ardalis.GuardClauses;
using CoreLib.Utils;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CoreLib.EntityFramework.Features.SearchPagination;

public static class QueryableExtensions
{
    /// <summary>
    /// Add pagination to query.
    /// Query must be ordered before using this method.
    /// </summary>
    /// <returns>Query.</returns>
    public static IQueryable<T> ApplyPagination<T>(
            this IQueryable<T> query, IPagedQuery pagedQuery)
    {
        Defend.Against.NotOrderedQuery(query);

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
    /// Query must be ordered before using this method.
    /// CAUTION: Set <paramref name="applyPagination"/> to false if used <see cref="ApplyPagination{T}(IQueryable{T}, IPagedQuery)"/>, otherwise it will double the pagination.
    /// </summary>
    /// <param name="applyPagination">If true – <see cref="ApplyPagination{T}(IQueryable{T}, IPagedQuery)"/> will be used to the query.</param>
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
        bool applyPagination = true,
        long? totalItemsRewrite = null!,
        CancellationToken cancellationToken = default)
    {
        Defend.Against.Null(query);
        Defend.Against.Null(pagedQuery);
        Defend.Against.NotOrderedQuery(query);
        if (applyPagination == false)
            Defend.Against.Null(totalItemsRewrite, message: $"Input {nameof(totalItemsRewrite)} required if {nameof(applyPagination)} is false");

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
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
            this IQueryable<T> query,
            ISearchQuery searchQuery,
            params Expression<Func<T, string?>>[] propsToSearch)
    {
        Defend.Against.LessElementsQuantity(propsToSearch, 1);
        Defend.Against.NullElements(propsToSearch);

        if (searchQuery is null || string.IsNullOrEmpty(searchQuery.SearchQuery))
        {
            return query;
        }

        // Create lambda for Where clause.
        var parameter = Expression.Parameter(typeof(T));

        // Build search string. Using string.Concat to concatenate multiple properties on the DB level.
        var concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) })!;
        var delimeter = " ";
        var expressionsToConcat = new List<Expression>();
        foreach (var prop in propsToSearch)
        {
            var (safeMemberAccessExpression, _) = ExpressionUtils.GetNestedMemberOrDefaultExpression(prop, string.Empty, parameter);
            expressionsToConcat.Add(Expression.Add(safeMemberAccessExpression, Expression.Constant(delimeter), concatMethodInfo));
        }
        var searchStringExpression = expressionsToConcat.Aggregate((leftExpr, rightExpr) => Expression.Add(leftExpr, rightExpr, concatMethodInfo));

        // Make it lower case
        var searchString = searchQuery.SearchQuery.Trim().ToLower();
        searchStringExpression = Expression.Call(searchStringExpression, nameof(string.ToLower), null);

        // Check if it contains searching query string.
        var searchingQueryExpression = Expression.Constant(searchString);
        var containsCallExpression = Expression.Call(searchStringExpression, nameof(string.Contains), null, searchingQueryExpression);

        // Create Where clause lambda.
        var whereLambda = Expression.Lambda(containsCallExpression, parameter);

        // Apply search
        query = query.Where((Expression<Func<T, bool>>)whereLambda);

        return query;
    }
}
