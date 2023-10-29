using System.Linq.Expressions;
using System.Reflection;

namespace CoreLib.Utils;

public static class ReflectionUtils
{
    /// <summary>
    /// Given a lambda expression that calls a method, returns the method info.
    /// </summary>
    public static MethodInfo GetMethodInfo(Expression<Action> expression)
        => GetMethodInfo((LambdaExpression)expression);

    /// <summary>
    /// Given a lambda expression that calls a method, returns the method info.
    /// </summary>
    public static MethodInfo GetMethodInfo(LambdaExpression expression)
    {
        if (expression.Body is not MethodCallExpression outermostExpression)
        {
            throw new ArgumentException("Invalid Expression. Expression should consists of a Method call only.");
        }

        return outermostExpression.Method;
    }
}
