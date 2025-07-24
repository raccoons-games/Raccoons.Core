using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage.Cryptography
{
    public class EncryptedStorageChannel : IStorageChannel
    {
        private IEncryptor _encryptor;
        private IDecryptor _decryptor;

        public EncryptedStorageChannel(IStorageChannel wrappedChannel, IEncryptor encryptor, IDecryptor decryptor, string keyAddition = "")
        {
            WrappedChannel = wrappedChannel;
            _encryptor = encryptor;
            _decryptor = decryptor;
            Key = keyAddition;
        }

        private IStorageChannel WrappedChannel { get; }
        public string Key { get; }

        private string WrapKey(string key)
        {
            return key + Key;
        }

        public void Delete(string key)
        {
            WrappedChannel.Delete(WrapKey(key));
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            return WrappedChannel.DeleteAsync(WrapKey(key), cancellationToken);
        }

        public bool Exists(string key)
        {
            return WrappedChannel.Exists(WrapKey(key));
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return WrappedChannel.ExistsAsync(WrapKey(key), cancellationToken);
        }

        public TData GetData<TData>(string key, System.Func<byte[], TData> decryption)
        {
            byte[] bytes = WrappedChannel.GetBytes(WrapKey(key));
            return decryption(bytes);
        }

        public async Task<TData> GetDataAsync<TData>(string key, System.Func<byte[], TData> decryption)
        {
            byte[] bytes = await WrappedChannel.GetBytesAsync(WrapKey(key));
            return decryption(bytes);
        }

        public void SetData<TData>(string key, TData value, System.Func<TData, byte[]> encryption)
        {
            byte[] bytes = encryption(value);
            WrappedChannel.SetBytes(key, bytes);
        }

        public Task SetDataAsync<TData>(string key, TData value, System.Func<TData, byte[]> encryption)
        {
            byte[] bytes = encryption(value);
            return WrappedChannel.SetBytesAsync(key, bytes);
        }

        public byte[] GetBytes(string key)
        {
            return GetData(WrapKey(key), _decryptor.Decrypt);
        }

        public Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync(WrapKey(key), _decryptor.Decrypt);
        }

        public float GetFloat(string key)
        {
            return GetData(WrapKey(key), _decryptor.DecryptFloat);
        }

        public Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync(WrapKey(key), _decryptor.DecryptFloat);
        }

        public int GetInt(string key)
        {
            return GetData(WrapKey(key), _decryptor.DecryptInt);
        }

        public Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync(WrapKey(key), _decryptor.DecryptInt);
        }

        public string GetString(string key)
        {
            return GetData(WrapKey(key), _decryptor.DecryptUTF8);
        }

        public Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync(WrapKey(key), _decryptor.DecryptUTF8);
        }

        public void SetBytes(string key, byte[] value)
        {
            SetData(WrapKey(key), value, _encryptor.Encrypt);
        }

        public Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(WrapKey(key), value, _encryptor.Encrypt);
        }

        public void SetFloat(string key, float value)
        {
            SetData(WrapKey(key), value, _encryptor.EncryptFloat);
        }

        public Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(WrapKey(key), value, _encryptor.EncryptFloat);
        }

        public void SetInt(string key, int value)
        {
            SetData(WrapKey(key), value, _encryptor.EncryptInt);
        }

        public Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(WrapKey(key), value, _encryptor.EncryptInt);
        }

        public void SetString(string key, string value)
        {
            SetData(WrapKey(key), value, _encryptor.EncryptUTF8);
        }

        public Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(WrapKey(key), value, _encryptor.EncryptUTF8);
        }
    }
}
