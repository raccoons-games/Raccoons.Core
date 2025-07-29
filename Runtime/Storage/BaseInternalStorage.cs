using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage
{
    public abstract class BaseInternalStorage : BaseStorage
    {
        public BaseInternalStorage(string key, IStorage parent) : base(key, parent)
        {
        }

        public override char Separator => System.IO.Path.AltDirectorySeparatorChar;

        public void Load()
        {
            string storageString = Parent.GetString(Key);
            Load(storageString);
        }

        public async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            string storageString = await Parent.GetStringAsync(Key, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            Load(storageString);
        }

        public void Save()
        {
            string storageString = StringifyStorage();
            Parent.SetString(Key, storageString);
        }

        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            string storageString = StringifyStorage();
            return Parent.SetStringAsync(Key, storageString, cancellationToken);
        }

        protected abstract string StringifyStorage();

        protected abstract void Load(string storageString);

        public override void Delete(string key)
        {
            DeleteInternal(key);
            Save();
        }

        protected abstract void DeleteInternal(string key);

        public override Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            DeleteInternal(key);
            return SaveAsync(cancellationToken);
        }

        public override Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetBytes(key));
        }


        public override Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetFloat(key));
        }

        public override Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetInt(key));
        }

        public override Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetString(key));
        }

        public override void SetBytes(string key, byte[] value)
        {
            SetBytesInternal(key, value);
            Save();
        }

        protected abstract void SetBytesInternal(string key, byte[] value);

        public override Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default)
        {
            SetBytesInternal(key, value);
            return SaveAsync(cancellationToken);
        }

        public override void SetFloat(string key, float value)
        {
            SetFloatInternal(key, value);
            Save();
        }

        protected abstract void SetFloatInternal(string key, float value);

        public override Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default)
        {
            SetFloatInternal(key, value);
            return SaveAsync(cancellationToken);
        }

        public override void SetInt(string key, int value)
        {
            SetIntInternal(key, value);
            Save();
        }

        protected abstract void SetIntInternal(string key, int value);

        public override Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
        {
            SetIntInternal(key, value);
            return SaveAsync(cancellationToken);
        }

        public override void SetString(string key, string value)
        {
            SetStringInternal(key, value);
            Save();
        }

        protected abstract void SetStringInternal(string key, string value);

        public override Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            SetStringInternal(key, value);
            return SaveAsync(cancellationToken);
        }

        public override bool OpenStorageForDebug()
        {
            if (Parent != null && Parent is BaseStorage baseStorage)
            {
                return baseStorage.OpenStorageForDebug();
            }
            return base.OpenStorageForDebug();
        }
    }
}