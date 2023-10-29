using CoreLib.Utils;
using System.Linq.Expressions;

namespace CoreLib.EntityFramework.Features.SearchPagination;

internal static class OrderingReflectionUtils
{
    private class OrderingTester : ExpressionVisitor
    {
        public bool orderingMethodFound;

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var name = node.Method.Name;

            if (node.Method.DeclaringType == typeof(Queryable) && 
                (name.StartsWith(nameof(Queryable.OrderBy), StringComparison.Ordinal)
                || name.StartsWith(nameof(Queryable.OrderByDescending), StringComparison.Ordinal)))
            {
                orderingMethodFound = true;
            }

            return base.VisitMethodCall(node);
        }
    }

    public static bool IsQueryOrdered(IQueryable query)
    {
        ArgumentUtils.MustBeNotNull(query, nameof(query));

        var visitor = new OrderingTester();
        visitor.Visit(query.Expression);
        return visitor.orderingMethodFound;
    }
}
