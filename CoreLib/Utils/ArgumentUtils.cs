using System.Collections;
using System.Numerics;

namespace CoreLib.Utils;

// TODO: use something other like GUARD instead of manually written methods.
public static class ArgumentUtils
{
    /// <summary>
    /// Returns default value if argument is null.
    /// Default value is null for reference types and default value for value types.
    /// </summary>
    /// <param name="defaultValue">Custom default value may be provided if needed.</param>
    /// <returns>The value.</returns>
    public static T DefaultIfNull<T>(T? argument, T defaultValue = default!)
    {
        argument ??= defaultValue;
        return argument;
    }

    public static void AllElementsMustBeNotNull<T>(this IEnumerable<T> argument, string argumentName)
    {
        if (argument is null)
        {
            return;
        }
        foreach (var element in argument)
        {
            if (element is null)
            {
                throw new ArgumentException($"All elements of {argumentName} must be not null.", argumentName);
            }
        }
    }

    public static void MustContainsAtLeast<T>(this IEnumerable<T> argument, long count, string argumentName)
    {
        MustBePositiveOrZero(count, nameof(count));
        if (argument is null)
        {
            throw new ArgumentNullException(argumentName);
        }
        if (argument.LongCount() < count)
        {
            throw new ArgumentException($"{argumentName} must contains at least {count} elements.", argumentName);
        }
    }

    public static void MustBePositive<T>(T argument, string name, string message = "Must be positive.")
        where T : INumber<T>
    {
        if (T.Sign(argument) <= 0)
        {
            throw new ArgumentException(name, message);
        }
    }

    public static void MustBePositiveOrZero<T>(T argument, string name, string message = "Must be positive or zero.")
        where T : INumber<T>
    {
        if (T.Sign(argument) < 0)
        {
            throw new ArgumentException(name, message);
        }
    }

    public static void MustBeNegative<T>(T argument, string name, string message = "Must be negative.")
        where T : INumber<T>
    {
        if (T.Sign(argument) >= 0)
        {
            throw new ArgumentException(name, message);
        }
    }

    public static void MustBeNegativeOrZero<T>(T argument, string name, string message = "Must be negative or zero.")
        where T : INumber<T>
    {
        if (T.Sign(argument) > 0)
        {
            throw new ArgumentException(name, message);
        }
    }
}
