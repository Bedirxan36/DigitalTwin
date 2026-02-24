using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DigitalTwin.Services.Security;

public class EncryptionService
{
    private byte[]? _key;
    private byte[]? _iv;

    public void InitializeKey(string masterPassword)
    {
        using var sha256 = SHA256.Create();
        _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(masterPassword));
        _iv = sha256.ComputeHash(Encoding.UTF8.GetBytes(masterPassword + "IV")).Take(16).ToArray();
    }

    public string Encrypt(string plainText)
    {
        if (_key == null || _iv == null)
            throw new InvalidOperationException("Encryption key not initialized");

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Decrypt(string cipherText)
    {
        if (_key == null || _iv == null)
            throw new InvalidOperationException("Encryption key not initialized");

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(cipherText);
        var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public bool IsInitialized => _key != null && _iv != null;
}
