namespace CoreLib.EntityFramework.Features.Encryption;

/// <summary>
/// Cryptographic converter.
/// </summary>
public interface ICryptoConverter
{
    /// <summary>
    /// Encrypt the value.
    /// </summary>
    /// <param name="value">Value.</param>
    string Encrypt(string value);

    /// <summary>
    /// Decrypt the value.
    /// </summary>
    /// <param name="value">Value.</param>
    string Decrypt(string value);

    /// <summary>
    /// Get maximal possible length of the encrypted value.
    /// </summary>
    int GetMaximalOverheadedLength(int originalLength);
}
