using CoreLib.EntityFramework.Features.Encryption.Exceptions;
using System.Security.Cryptography;
using System.Text;

namespace CoreLib.EntityFramework.Features.Encryption;

/// <summary>
/// Cryptographic converter using overloaded AES encryption.
/// </summary>
public sealed class DefaultCryptoConverter : ICryptoConverter
{
    private readonly byte[] cryptKey;
    private readonly byte[] authKey;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cryptKey">Encryption key. !!! Must be size one of: 16, 24, 32 symbols. !!!</param>
    /// <param name="authKey">Auth key.</param>
    public DefaultCryptoConverter(string cryptKey, string authKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(cryptKey, nameof(cryptKey));
        ArgumentException.ThrowIfNullOrEmpty(authKey, nameof(authKey));

        this.cryptKey = Encoding.UTF8.GetBytes(cryptKey);
        this.authKey = Encoding.UTF8.GetBytes(authKey);
    }

    private Aes BuildAes()
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Key = cryptKey;
        return aes;
    }

    /// <inheritdoc/>
    public int GetMaximalOverheadedLength(int originalLength)
    {
        using var aes = BuildAes();
        var blockSizeInBytes = aes.BlockSize / 8;
        const int hmacTagLength = 32;
        const float base64Multiplicator = 1.35f;

        var messagePaddedToBlockSizeLength = blockSizeInBytes * Math.Ceiling((float)originalLength / blockSizeInBytes);

        return (int)Math.Ceiling(
            (blockSizeInBytes + messagePaddedToBlockSizeLength + hmacTagLength) * base64Multiplicator);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Adds overhead of (BlockSize(16) + Message-Padded-To-Blocksize +  HMac-Tag(32)) * 1.33 Base64.
    /// </remarks>
    public string Encrypt(string value)
    {
        if (value == null)
        {
            return null!;
        }

        using var aes = BuildAes();

        byte[] encryptedText;
        var iv = aes.IV;

        using (var encrypter = aes.CreateEncryptor(cryptKey, iv))
        using (var cipherStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
            using (var binaryWriter = new BinaryWriter(cryptoStream))
            {
                // Encrypt data.
                binaryWriter.Write(value);
            }

            encryptedText = cipherStream.ToArray();
        }

        // Assemble encrypted data and add authentication.
        using (var hmac = new HMACSHA256(authKey))
        using (var encryptedStream = new MemoryStream())
        using (var binaryWriter = new BinaryWriter(encryptedStream))
        {
            // Prepend IV.
            binaryWriter.Write(iv);
            // Write encrypted data.
            binaryWriter.Write(encryptedText);
            binaryWriter.Flush();

            // Authenticate all data.
            var tag = hmac.ComputeHash(encryptedStream.ToArray());

            // Append tag.
            binaryWriter.Write(tag);
            binaryWriter.Flush();

            encryptedStream.Position = 0;
            return Convert.ToBase64String(encryptedStream.ToArray());
        }
    }

    /// <inheritdoc/>
    public string Decrypt(string value)
    {
        if (value == null)
        {
            return null!;
        }

        byte[] encryptedMessage;
        try
        {
            encryptedMessage = Convert.FromBase64String(value);
        }
        catch (FormatException e)
        {
            throw new DecryptionException("The provided value is not valid. It's not a base64 string.", e);
        }

        using var aes = BuildAes();
        using var hmac = new HMACSHA256(authKey);

        var sentTag = new byte[hmac.HashSize / 8];
        byte[] calcTag;

        // Calculate tag.
        try
        {
            calcTag = hmac.ComputeHash(encryptedMessage, 0, encryptedMessage.Length - sentTag.Length);
        }
        catch (ArgumentException e)
        {
            throw new DecryptionException("The provided value is not valid. Hash cannot be computed.", e);
        }

        // Check the value length.
        var ivLength = aes.BlockSize / 8;
        if (encryptedMessage.Length < sentTag.Length + ivLength)
        {
            throw new DecryptionException("The provided value is not valid. It is too long to be decrypted.");
        }

        // Grab sent tag.
        Array.Copy(encryptedMessage, encryptedMessage.Length - sentTag.Length, sentTag, 0, sentTag.Length);

        // Compare tag with constant time comparison.
        var compare = 0;
        for (var i = 0; i < sentTag.Length; i++)
        {
            compare |= sentTag[i] ^ calcTag[i];
        }
        // Authenticate the value.
        if (compare != 0)
        {
            throw new DecryptionException("The provided value is not valid. Authentication was fail.");
        }

        // Grab IV from the value.
        var iv = new byte[ivLength];
        Array.Copy(encryptedMessage, iv, iv.Length);

        aes.IV = iv;

        using var plainTextStream = new MemoryStream();
        using (var decryptor = aes.CreateDecryptor())
        using (var decryptorStream = new CryptoStream(plainTextStream, decryptor, CryptoStreamMode.Write))
        using (var binaryWriter = new BinaryWriter(decryptorStream))
        {
            // Decrypt the value.
            try
            {
                binaryWriter.Write(encryptedMessage, iv.Length, encryptedMessage.Length - iv.Length - sentTag.Length);
            }
            catch (CryptographicException e)
            {
                throw new DecryptionException("The provided value cannot be decrypted.", e);
            }
        }

        // Return plain text. The first character is always '\n' character.
        return Encoding.UTF8.GetString(plainTextStream.ToArray())[1..];
    }
}
