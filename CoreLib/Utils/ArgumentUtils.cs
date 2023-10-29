using System.Collections;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CoreLib.Utils;

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

    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if argument is null.
    /// </summary>
    /// <param name="name">Name of argument: <see cref="nameof"/></param>
    public static void MustBeNotNull<T>(T argument, string name)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(name);
        }
    }

    public static void AllMustBeNotNull<T>(T argument, string name)
        where T : IEnumerable
    {
        if (argument is null)
        {
            throw new ArgumentNullException(name);
        }
        foreach (var element in argument)
        {
            if (element is null)
            {
                throw new ArgumentException($"All elements of {name} must be not null.", name);
            }
        }
    }

    public static void MustBePositive<T>(T argument, string name, string message = "Must be positive.")
        where T: INumber<T>
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
