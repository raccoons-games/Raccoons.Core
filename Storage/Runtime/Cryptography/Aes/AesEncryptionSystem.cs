using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Raccoons.Storage.Cryptography.Aes
{

    public class AesEncryption : IEncryptor, IDecryptor
    {
        private object _providerLock = new object();
        private readonly AesCryptoServiceProvider _aesCryptoProvider;

        public AesEncryption(string key, string iv, int blockSize = 16,
            CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.PKCS7)
            : this(Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(iv), blockSize, cipherMode, paddingMode)
        {

        }

        public AesEncryption(byte[] key, byte[] iv, int blockSize = 16,
            CipherMode cipherMode = CipherMode.CBC, PaddingMode paddingMode = PaddingMode.PKCS7)
        {
            _aesCryptoProvider = new AesCryptoServiceProvider();
            _aesCryptoProvider.BlockSize = blockSize * 8;
            _aesCryptoProvider.KeySize = key.Length * 8;
            _aesCryptoProvider.Key = key;
            _aesCryptoProvider.IV = iv;
            _aesCryptoProvider.Mode = cipherMode;
            _aesCryptoProvider.Padding = paddingMode;
        }

        public byte[] Encrypt(byte[] input)
        {
            ICryptoTransform encryptorTransform;
            lock (_providerLock)
            {
                encryptorTransform = _aesCryptoProvider.CreateEncryptor();
            }
            return encryptorTransform.TransformFinalBlock(input, 0, input.Length);
        }

        public byte[] Decrypt(byte[] input)
        {
            ICryptoTransform decryptorTransform;
            lock (_providerLock)
            {
                decryptorTransform = _aesCryptoProvider.CreateDecryptor();
            }
            return decryptorTransform.TransformFinalBlock(input, 0, input.Length);
        }
    }
}
