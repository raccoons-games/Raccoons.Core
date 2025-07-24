using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Raccoons.Storage.Cryptography;
using Raccoons.Storage.Cryptography.Aes;
using UnityEngine;
using UnityEngine.TestTools;

namespace Raccoons.Storage.Tests
{
    public class EncryptionTests
    {
        [Test]
        public void AesEncryptionTest()
        {
            AesEncryption aesEncryption = new AesEncryption(
                EncryptionExtensions.GenerateKey(32), EncryptionExtensions.GenerateKey(16));
            string str = EncryptionExtensions.GenerateKey(128);
            byte[] encrypted = aesEncryption.EncryptUTF8(str);
            string decrypted = aesEncryption.DecryptUTF8(encrypted);
            Assert.AreEqual(str, decrypted);

            Debug.Log("AES string passed");

            PlayerPrefsStorage storage = new PlayerPrefsStorage("Raccoons/Storage/Tests");
            EncryptedStorageChannel encryptedStorageChannel = new EncryptedStorageChannel(storage, aesEncryption, aesEncryption);
            encryptedStorageChannel.SetFloat("Pi", Mathf.PI);
            float decryptedPi = encryptedStorageChannel.GetFloat("Pi");
            Assert.AreEqual(Mathf.PI, decryptedPi, 0.00001f);

            encryptedStorageChannel.SetInt("MaxInt", int.MaxValue);
            int decryptedInt = encryptedStorageChannel.GetInt("MaxInt");
            Assert.AreEqual(int.MaxValue, decryptedInt);

            Debug.Log("AES storage channel passed");
        }


    }
}