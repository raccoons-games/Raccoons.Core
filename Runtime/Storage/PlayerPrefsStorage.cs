using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Storage
{
    /// <summary>
    /// Storage that stores its values in PlayerPrefs
    /// </summary>
    public class PlayerPrefsStorage : BaseFullPathStorage
    {
        public PlayerPrefsStorage(string key, IStorage parent = null) : base(key, parent)
        {
        }

        public override char Separator => '/';

        protected override Task DeleteAsyncInternal(string fullPath, CancellationToken cancellationToken = default)
        {
            DeleteInternal(fullPath);
            return Task.CompletedTask;
        }

        protected override void DeleteInternal(string fullPath)
        {
            PlayerPrefs.DeleteKey(fullPath);
        }

        protected override Task<bool> ExistsAsyncInternal(string fullPath, CancellationToken cancellationToken = default)
        {
            bool result = ExistsInternal(fullPath);
            return Task.FromResult(result);
        }

        protected override bool ExistsInternal(string fullPath)
        {
            return PlayerPrefs.HasKey(fullPath);
        }

        protected override Task<byte[]> GetBytesAsyncInternal(string key, CancellationToken cancellationToken)
        {
            byte[] result = GetBytesInternal(key);
            return Task.FromResult(result);
        }

        protected override byte[] GetBytesInternal(string key)
        {
            string base64 = GetStringInternal(key);
            return Convert.FromBase64String(base64);
        }

        protected override Task<float> GetFloatAsyncInternal(string fullPath, CancellationToken cancellationToken = default)
        {
            float result = GetFloatInternal(fullPath);
            return Task.FromResult(result);
        }

        protected override float GetFloatInternal(string fullPath)
        {
            return PlayerPrefs.GetFloat(fullPath);
        }

        protected override Task<int> GetIntAsyncInternal(string fullPath, CancellationToken cancellationToken = default)
        {
            int result = GetIntInternal(fullPath);
            return Task.FromResult(result);
        }

        protected override int GetIntInternal(string fullPath)
        {
            return PlayerPrefs.GetInt(fullPath);
        }

        protected override Task<string> GetStringAsyncInternal(string fullPath, CancellationToken cancellationToken = default)
        {
            string result = GetStringInternal(fullPath);
            return Task.FromResult(result);
        }

        protected override string GetStringInternal(string fullPath)
        {
            return PlayerPrefs.GetString(fullPath);
        }

        protected override Task SetBytesAsyncInternal(string key, byte[] value, CancellationToken cancellationToken)
        {
            SetBytesInternal(key, value);
            return Task.CompletedTask;
        }

        protected override void SetBytesInternal(string key, byte[] value)
        {
            string base64 = Convert.ToBase64String(value);
            SetStringInternal(key, base64);
        }

        protected override Task SetFloatAsyncInternal(string fullPath, float value, CancellationToken cancellationToken = default)
        {
            SetFloatInternal(fullPath, value);
            return Task.CompletedTask;
        }

        protected override void SetFloatInternal(string fullPath, float value)
        {
            PlayerPrefs.SetFloat(fullPath, value);
        }

        protected override Task SetIntAsyncInternal(string fullPath, int value, CancellationToken cancellationToken = default)
        {
            SetIntInternal(fullPath, value);
            return Task.CompletedTask;
        }

        protected override void SetIntInternal(string fullPath, int value)
        {
            PlayerPrefs.SetInt(fullPath, value);
        }

        protected override Task SetStringAsyncInternal(string fullPath, string value, CancellationToken cancellationToken = default)
        {
            SetStringInternal(fullPath, value);
            return Task.CompletedTask;
        }

        protected override void SetStringInternal(string fullPath, string value)
        {
            PlayerPrefs.SetString(fullPath, value);
        }

        public override bool OpenStorageForDebug()
        {
            Debug.Log($"[{GetType().Name}] Cannot open PlayerPrefs-storage for debugging. More details about how to access it: https://docs.unity3d.com/ScriptReference/PlayerPrefs.html");
            return base.OpenStorageForDebug();
        }
    }
}