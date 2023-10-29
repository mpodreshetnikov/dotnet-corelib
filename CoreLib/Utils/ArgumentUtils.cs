namespace CoreLib.Utils;

public static class ArgumentUtils
{
    /// <summary>
    /// Returns default value if argument is null.
    /// Default value is null for reference types and default value for value types.
    /// </summary>
    /// <param name="defaultValue">Custom default value may be provided if needed.</param>
    /// <returns>The value.</returns>
    public static T DefaultIfNull<T>(T argument, T defaultValue = default!)
    {
        argument ??= defaultValue;
        return argument;
    }

    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if argument is null.
    /// </summary>
    /// <param name="name">Name of argument: <see cref="nameof"/></param>
    public static void ThrowIfNull<T>(T argument, string name)
    {
        if (argument is null)
        {
            throw new ArgumentNullException(name);
        }
    }
}
