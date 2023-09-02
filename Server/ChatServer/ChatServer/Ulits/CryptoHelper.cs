using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    public static string Encrypt(string authority, string key, string? initialVector = null)
    {
        var keyInBytes = Encoding.ASCII.GetBytes(key);

        using (var aes = Aes.Create())
        {
            // KeySize depends on Key length
            aes.Key = keyInBytes;
            aes.Mode = CipherMode.CBC;


            if (initialVector == null)
            {
                var initialVectorInBytes = new byte[aes.BlockSize / 8];

                using (var rngCryptoServiceProvider = RandomNumberGenerator.Create())
                {
                    rngCryptoServiceProvider.GetBytes(initialVectorInBytes);
                }

                aes.IV = initialVectorInBytes;
            }
            else
            {
                aes.IV = Encoding.ASCII.GetBytes(initialVector);
            }

            using (var encryptor = aes.CreateEncryptor())
            {
                using (var memStream = new MemoryStream())
                {
                    using (var crytoStream = new CryptoStream(memStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(crytoStream, Encoding.UTF8))
                        {
                            sw.Write(authority);
                        }
                    }

                    var encryptedText = memStream.ToArray();

                    // Combine the IV and encrypted data into a single byte array
                    var combinedBytes = new byte[aes.IV.Length + encryptedText.Length];
                    Buffer.BlockCopy(aes.IV, 0, combinedBytes, 0, aes.IV.Length);
                    Buffer.BlockCopy(encryptedText, 0, combinedBytes, aes.IV.Length, encryptedText.Length);

                    return Convert.ToBase64String(combinedBytes);
                }
            }
        }
    }



    public static string Decrypt(string encryptedAuthority, string key)
    {
        var keyInBytes = Encoding.ASCII.GetBytes(key);
        var encryptedPasswordInBytes = Convert.FromBase64String(encryptedAuthority);

        using (var aes = Aes.Create())
        {
            aes.Key = keyInBytes;
            aes.Mode = CipherMode.CBC;

            using (var memStream = new MemoryStream(encryptedPasswordInBytes))
            {
                var iv = new byte[aes.BlockSize / 8];
                memStream.Read(iv, 0, iv.Length);
                aes.IV = iv;

                // Create an encryptor to perform the stream transform.
                using (var decrytor = aes.CreateDecryptor())
                {
                    using (var crytoStream = new CryptoStream(memStream, decrytor, CryptoStreamMode.Read))
                    {
                        // Here we are setting the Encoding
                        using (var sr = new StreamReader(crytoStream, Encoding.UTF8))
                        {
                            // Read all data from the stream.
                            var plainText = sr.ReadToEnd();
                            return plainText;
                        }
                    }
                }
            }
        }
    }
}