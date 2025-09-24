using System.Security.Cryptography;
using System.Text;

public static class AesEncryptionHelper
{
    public static string Encrypt(string plainText, string key)
    {
        using var aes = Aes.Create();
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] result = new byte[aes.IV.Length + encrypted.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }
    public static string Decrypt(string cipherTextBase64, string key)
    {
        byte[] fullCipher = Convert.FromBase64String(cipherTextBase64);

        // Tạo đối tượng AES
        using var aes = Aes.Create();

        // Lấy IV (Initialization Vector) đầu
        int ivLength = aes.BlockSize / 8;
        byte[] iv = new byte[ivLength];
        byte[] cipher = new byte[fullCipher.Length - ivLength];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        // Tạo khóa từ password
        aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        aes.IV = iv;

        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        byte[] decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(decrypted);
    }
}
