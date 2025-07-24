using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Storage.Instances
{
    public abstract class BaseStorageAsset : ScriptableObject, IStorage
    {
        [SerializeField]
        private BaseStorageAsset parent;

        [SerializeField]
        private string key;

        private IStorage _storage;

        public IStorage Storage 
        {
            get => _storage ??= CreateStorage(key, parent?.Storage);
        }

        public string Path => Storage.Path;

        public IStorage Parent { get => Storage.Parent; set => Storage.Parent = value; }

        public IEnumerable<IStorage> Children => Storage.Children;

        public string Key => Storage.Key;

        public void AddChild(IStorage child)
        {
            Storage.AddChild(child);
        }

        public void Delete(string key)
        {
            Storage.Delete(key);
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            return Storage.DeleteAsync(key, cancellationToken);
        }

        public bool Exists(string key)
        {
            return Storage.Exists(key);
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Storage.ExistsAsync(key, cancellationToken);
        }

        public byte[] GetBytes(string key)
        {
            return Storage.GetBytes(key);
        }

        public Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            return Storage.GetBytesAsync(key, cancellationToken);
        }

        public float GetFloat(string key)
        {
            return Storage.GetFloat(key);
        }

        public Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default)
        {
            return Storage.GetFloatAsync(key, cancellationToken);
        }

        public int GetInt(string key)
        {
            return Storage.GetInt(key);
        }

        public Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default)
        {
            return Storage.GetIntAsync(key, cancellationToken);
        }

        public string GetString(string key)
        {
            return Storage.GetString(key);
        }

        public Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            return Storage.GetStringAsync(key, cancellationToken);
        }

        public void RemoveChild(IStorage child)
        {
            Storage.RemoveChild(child);
        }

        public void SetBytes(string key, byte[] value)
        {
            Storage.SetBytes(key, value);
        }

        public Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default)
        {
            return Storage.SetBytesAsync(key, value, cancellationToken);
        }

        public void SetFloat(string key, float value)
        {
            Storage.SetFloat(key, value);
        }

        public Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default)
        {
            return Storage.SetFloatAsync(key, value, cancellationToken);
        }

        public void SetInt(string key, int value)
        {
            Storage.SetInt(key, value);
        }

        public Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
        {
            return Storage.SetIntAsync(key, value, cancellationToken);
        }

        public void SetString(string key, string value)
        {
            Storage.SetString(key, value);
        }

        public Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            return Storage.SetStringAsync(key, value, cancellationToken);
        }

        protected abstract IStorage CreateStorage(string key, IStorage parent);
    }
}
