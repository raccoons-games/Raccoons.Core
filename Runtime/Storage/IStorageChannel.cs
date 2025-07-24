using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage
{
    public interface IStorageChannel
    {
        string Key { get; }

        void Delete(string key);
        Task DeleteAsync(string key, CancellationToken cancellationToken = default);
        bool Exists(string key);
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        byte[] GetBytes(string key);
        Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default);
        float GetFloat(string key);
        Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default);
        int GetInt(string key);
        Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default);
        string GetString(string key);
        Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default);
        void SetBytes(string key, byte[] value);
        Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default);
        void SetFloat(string key, float value);
        Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default);
        void SetInt(string key, int value);
        Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default);
        void SetString(string key, string value);
        Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default);
    }
}