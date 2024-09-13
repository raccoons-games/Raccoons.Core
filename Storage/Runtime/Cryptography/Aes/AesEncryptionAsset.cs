using System.Security.Cryptography;
using UnityEngine;

namespace Raccoons.Storage.Cryptography.Aes
{
    [CreateAssetMenu(fileName = "AesEncryptionAsset", menuName = "Raccoons/Storage/Encryption/AesEncryptionAsset")]
    public class AesEncryptionAsset : ScriptableObject, IEncryptor, IDecryptor
    {
        [SerializeField]
        private int keyLength = 32;

        [SerializeField]
        private string key;

        [SerializeField]
        private string iv;

        [SerializeField]
        private int blockSize = 16;

        [SerializeField]
        private CipherMode cipherMode = CipherMode.CBC;

        [SerializeField]
        private PaddingMode paddingMode = PaddingMode.PKCS7;

        public AesEncryption AesEncryption { get; private set; }

        public byte[] Decrypt(byte[] input)
        {
            return ((IDecryptor)AesEncryption).Decrypt(input);
        }

        public byte[] Encrypt(byte[] input)
        {
            return ((IEncryptor)AesEncryption).Encrypt(input);
        }

        private void OnEnable()
        {
            AesEncryption = new AesEncryption(key, iv, blockSize, cipherMode, paddingMode);
        }

#if UNITY_EDITOR
        public void Verify(bool askToAdjust = true)
        {
            var newBlockSize = Mathf.ClosestPowerOfTwo(blockSize);
            var newKeyLength = Mathf.ClosestPowerOfTwo(keyLength);
            int strLen = newKeyLength;
            var newKey = EncryptionExtensions.SupportLength(key, strLen);
            var newIV = EncryptionExtensions.SupportLength(iv, 16);
            if (newBlockSize != blockSize || newKeyLength != keyLength || newIV != iv || newKey != key)
            {
                if (!askToAdjust || UnityEditor.EditorUtility.DisplayDialog("Adjust key?", "The key doesn't fit requirements. Do you want to adjust it automatically?", "Yes", "No"))
                {
                    blockSize = newBlockSize;
                    keyLength = newKeyLength;
                    iv = newIV;
                    key = newKey;
                    UnityEditor.EditorUtility.SetDirty(this);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
        }

        public void Regenerate()
        {
            key = "";
            iv = "";
            Verify(false);
        }
#endif
    }
}
