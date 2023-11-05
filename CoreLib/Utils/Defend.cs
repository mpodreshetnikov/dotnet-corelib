using Ardalis.GuardClauses;
using System.Runtime.CompilerServices;
namespace CoreLib.Utils;

public static class Defend
{
    public static IGuardClause Against => Guard.Against;
}

public static class DefendExtensions
{
    /// <summary>
    /// Returns default value if argument is null.
    /// Default value is null for reference types and default value for value types.
    /// </summary>
    /// <param name="defaultValue">Custom default value may be provided if needed.</param>
    /// <returns>The value.</returns>
    public static T NullOrReturnDefault<T>(
        this IGuardClause _,
        T? input,
        T defaultValue = default!)
    {
        return input ?? defaultValue;
    }

    public static void NullElements<T>(
       this IGuardClause _,
       IEnumerable<T> input,
       [CallerArgumentExpression(nameof(input))] string? parameterName = null,
       string? message = null)
    {
        if (input is null)
        {
            return;
        }
        foreach (var element in input)
        {
            if (element is null)
            {
                throw new ArgumentException(message ?? $"All elements of input {parameterName} must be not null.", parameterName);
            }
        }
    }

    public static void LessElementsQuantity<T>(
        this IGuardClause _,
        IEnumerable<T> input,
        long minimalElementsQuantity,
        [CallerArgumentExpression(nameof(input))] string? parameterName = null,
        string? message = null)
    {
        Defend.Against.NegativeOrZero(minimalElementsQuantity);
        Defend.Against.Null(input, parameterName);

        if (input.LongCount() < minimalElementsQuantity)
        {
            throw new ArgumentException(message ?? $"Input {parameterName} must contains at least {minimalElementsQuantity} elements.", parameterName);
        }
    }

    public static void NotOrderedQuery(
        this IGuardClause _,
        IQueryable input,
        [CallerArgumentExpression(nameof(input))] string? parameterName = null,
        string? message = null)
    {
        Defend.Against.Null(input, parameterName);

        if (ReflectionUtils.IsQueryOrdered(input) == false)
        {
            throw new ArgumentException(message ?? $"Input {parameterName} must be ordered.", parameterName);
        }
    }
}
