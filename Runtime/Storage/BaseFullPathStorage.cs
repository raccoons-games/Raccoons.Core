using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage
{
    public abstract class BaseFullPathStorage : BaseStorage
    {
        public BaseFullPathStorage(string key, IStorage parent = null) : base(key, parent)
        {
        }

        public override Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return GetStringAsyncInternal(fullPath, cancellationToken);
        }

        protected abstract Task<string> GetStringAsyncInternal(string fullPath, CancellationToken cancellationToken = default);

        public override Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return GetIntAsyncInternal(fullPath, cancellationToken);
        }

        protected abstract Task<int> GetIntAsyncInternal(string fullPath, CancellationToken cancellationToken = default);

        public override Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return GetFloatAsyncInternal(fullPath, cancellationToken);
        }

        protected abstract Task<float> GetFloatAsyncInternal(string fullPath, CancellationToken cancellationToken = default);

        public override Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return SetStringAsyncInternal(fullPath, value, cancellationToken);
        }

        protected abstract Task SetStringAsyncInternal(string fullPath, string value, CancellationToken cancellationToken = default);

        public override Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return SetIntAsyncInternal(fullPath, value, cancellationToken);
        }

        protected abstract Task SetIntAsyncInternal(string fullPath, int value, CancellationToken cancellationToken = default);

        public override Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return SetFloatAsyncInternal(fullPath, value, cancellationToken);
        }

        protected abstract Task SetFloatAsyncInternal(string fullPath, float value, CancellationToken cancellationToken = default);

        public override Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return ExistsAsyncInternal(fullPath, cancellationToken);
        }

        protected abstract Task<bool> ExistsAsyncInternal(string fullPath, CancellationToken cancellationToken = default);

        public override Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return DeleteAsyncInternal(fullPath, cancellationToken);
        }

        protected abstract Task DeleteAsyncInternal(string fullPath, CancellationToken cancellationToken = default);

        public override string GetString(string key)
        {
            string fullPath = PathOf(key);
            return GetStringInternal(fullPath);
        }

        protected abstract string GetStringInternal(string fullPath);

        public override int GetInt(string key)
        {
            string fullPath = PathOf(key);
            return GetIntInternal(fullPath);
        }

        protected abstract int GetIntInternal(string fullPath);

        public override float GetFloat(string key)
        {
            string fullPath = PathOf(key);
            return GetFloatInternal(fullPath);
        }

        protected abstract float GetFloatInternal(string fullPath);

        public override void SetString(string key, string value)
        {
            string fullPath = PathOf(key);
            SetStringInternal(fullPath, value);
        }

        protected abstract void SetStringInternal(string fullPath, string value);

        public override void SetFloat(string key, float value)
        {
            string fullPath = PathOf(key);
            SetFloatInternal(fullPath, value);
        }

        protected abstract void SetFloatInternal(string fullPath, float value);

        public override void SetInt(string key, int value)
        {
            string fullPath = PathOf(key);
            SetIntInternal(fullPath, value);
        }

        protected abstract void SetIntInternal(string fullPath, int value);

        public override bool Exists(string key)
        {
            string fullPath = PathOf(key);
            return ExistsInternal(fullPath);
        }

        protected abstract bool ExistsInternal(string fullPath);

        public override void Delete(string key)
        {
            string fullPath = PathOf(key);
            DeleteInternal(fullPath);
        }

        protected abstract void DeleteInternal(string fullPath);

        public override Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return GetBytesAsyncInternal(fullPath, cancellationToken);
        }

        protected abstract Task<byte[]> GetBytesAsyncInternal(string key, CancellationToken cancellationToken);

        public override Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default)
        {
            string fullPath = PathOf(key);
            return SetBytesAsyncInternal(fullPath, value, cancellationToken);
        }

        protected abstract Task SetBytesAsyncInternal(string key, byte[] value, CancellationToken cancellationToken);

        public override byte[] GetBytes(string key)
        {
            string fullPath = PathOf(key);
            return GetBytesInternal(fullPath);
        }

        protected abstract byte[] GetBytesInternal(string key);

        public override void SetBytes(string key, byte[] value)
        {
            string fullPath = PathOf(key);
            SetBytesInternal(fullPath, value);
        }

        protected abstract void SetBytesInternal(string key, byte[] value);
    }
}