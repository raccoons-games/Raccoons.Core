using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage.Memory
{
    public class SingleDataMemoryStorageChannel : IStorageChannel
    {
        private object _data = default;

        public SingleDataMemoryStorageChannel(object initialData, string initialKey)
        {
            _data = initialData;
            Key = initialKey;
        }

        public SingleDataMemoryStorageChannel() : this(default, default)
        {

        }

        public string Key { get; private set; }

        public void Delete(string key)
        {
            if (!string.IsNullOrEmpty(Key) && Key.Equals(Key))
            {
                _data = default;
                Key = default;
            }
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            Delete(key);
            return Task.CompletedTask;
        }

        public bool Exists(string key)
        {
            return (!string.IsNullOrEmpty(Key) && Key.Equals(key));
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Exists(key));
        }

        public TData GetData<TData>(string key)
        {
            if (!string.IsNullOrEmpty(Key) && Key.Equals(key))
            {
                if (_data is TData castedData)
                {
                    return castedData;
                }
            }
            return default;
        }

        public Task<TData> GetDataAsync<TData>(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetData<TData>(key));
        }

        public byte[] GetBytes(string key)
        {
            return GetData<byte[]>(key);
        }

        public Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync<byte[]>(key, cancellationToken);
        }

        public float GetFloat(string key)
        {
            return GetData<float>(key);
        }

        public Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync<float>(key, cancellationToken);
        }

        public int GetInt(string key)
        {
            return GetData<int>(key);
        }

        public Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync<int>(key, cancellationToken);
        }

        public string GetString(string key)
        {
            return GetData<string>(key);
        }

        public Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetDataAsync<string>(key, cancellationToken);
        }

        public void SetData(string key, object data)
        {
            Key = key;
            _data = data;
        }

        public Task SetDataAsync(string key, object data)
        {
            SetData(key, data);
            return Task.CompletedTask;
        }

        public void SetBytes(string key, byte[] value)
        {
            SetData(key, value);
        }

        public Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(key, value);
        }

        public void SetFloat(string key, float value)
        {
            SetData(key, value);
        }

        public Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(key, value);
        }

        public void SetInt(string key, int value)
        {
            SetData(key, value);
        }

        public Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(key, value);
        }

        public void SetString(string key, string value)
        {
            SetData(key, value);
        }

        public Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            return SetDataAsync(key, value);
        }
    }
}
