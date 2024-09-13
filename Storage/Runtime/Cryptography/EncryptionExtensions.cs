using System.Text;
using UnityEngine;

namespace Raccoons.Storage.Cryptography
{
    public static class EncryptionExtensions
    {
        public const string KEY_CHARSET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static byte[] EncryptUTF8(this IEncryptor encryptor, string str)
        {
            return encryptor.Encrypt(Encoding.UTF8.GetBytes(str));
        }

        public static string DecryptUTF8(this IDecryptor decryptor, byte[] bytes)
        {
            return Encoding.UTF8.GetString(decryptor.Decrypt(bytes));
        }

        public static byte[] EncryptInt(this IEncryptor encryptor, int number)
        {
            return EncryptUTF8(encryptor, number.ToString());
        }

        public static int DecryptInt(this IDecryptor decryptor, byte[] bytes)
        {
            return int.Parse(DecryptUTF8(decryptor, bytes));
        }

        public static byte[] EncryptFloat(this IEncryptor encryptor, float number)
        {
            return EncryptUTF8(encryptor, number.ToString());
        }

        public static float DecryptFloat(this IDecryptor decryptor, byte[] bytes)
        {
            return float.Parse(DecryptUTF8(decryptor, bytes));
        }

        public static string SupportLength(string str, int length)
        {
            if (str.Length < length)
            {
                char[] charset = GetCharset();
                while (str.Length < length)
                {
                    str += charset[Random.Range(0, charset.Length)];
                }
            }
            else if (str.Length > length)
            {
                str = str.Remove(length);
            }
            return str;
        }

        public static string GenerateKey(int length)
        {
            string key = "";
            return SupportLength(key, length);
        }

        public static char[] GetCharset()
        {
            return KEY_CHARSET.ToCharArray();
        }
    }

}
