namespace CoreLib.Utils;

public static class AtomicUtils
{
    /// <summary>
    /// Safely concatenates strings using the specified separator between each member.
    /// Skips null or empty strings without throwing exceptions.
    /// </summary>
    /// <param name="separator">Separator.</param>
    /// <param name="strings">String to concatenate.</param>
    /// <returns>Concatenated string.</returns>
    public static string SafelyJoin(string separator, params string[] strings)
        => string.Join(separator, strings?.Where(s => !string.IsNullOrEmpty(s)) ?? Array.Empty<string>());
    
    /// <summary>
    /// Safely concatenates strings using the specified separator between each member.
    /// Skips null or empty strings without throwing exceptions.
    /// </summary>
    /// <param name="separator">Separator.</param>
    /// <param name="strings">String to concatenate.</param>
    /// <returns>Concatenated string.</returns>
    public static string SafelyJoin(string separator, IEnumerable<string> strings)
        => SafelyJoin(separator, strings?.ToArray() ?? Array.Empty<string>());
}
