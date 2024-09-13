using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage.Files
{
    public abstract class BaseFileSystemStorage : BaseStorage
    {
        public FileStorageSettings Settings { get; private set; }

        protected BaseFileSystemStorage(string key, FileStorageSettings settings, IStorage parent) : base(key, parent)
        {
            Settings = settings;
        }

        public override IStorage Parent
        {
            get => base.Parent;
            set
            {
                base.Parent = value;
                if (value != null)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Path);
                    string parentPath = directoryInfo.Parent.ToString();
                    if (!Directory.Exists(parentPath))
                    {
                        Directory.CreateDirectory(parentPath);
                    }
                }
            }
        }

        public override char Separator => System.IO.Path.AltDirectorySeparatorChar;

        protected abstract Task Schedule(System.Func<CancellationToken, Task> task, string key = null, CancellationToken token = default);

        protected Task Schedule(System.Action action, string key = null, CancellationToken token = default)
        {
            return Schedule((token) =>
            {
                if (!token.IsCancellationRequested)
                {
                    try
                    {
                        action();
                        return Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        return Task.FromException(ex);
                    }
                }
                else return Task.FromCanceled(token);
            }, key, token).ContinueWith(task =>
            {
                if (task.IsFaulted) throw task.Exception;
            });
        }

        protected abstract void WaitForTopTask(string key = null);

        protected virtual async Task ApplyChanges(System.Action action, string key = null)
        {
            if (Settings.AsyncBlocksSync)
            {
                WaitForTopTask(key);
                action();
            }
            else
            {
                await Schedule(action, key);
            }
        }

        protected virtual async Task ApplyChangesAsync(System.Action action, string key = null, CancellationToken cancellationToken = default)
        {
            await Schedule(action, key, cancellationToken);
        }

        protected T GetValue<T>(System.Func<T> func, string key = null)
        {
            if (Settings.AsyncBlocksSync)
            {
                WaitForTopTask(key);
            }
            return func();
        }

        protected async Task<T> GetValueAsync<T>(System.Func<T> func, string key = null, CancellationToken cancellationToken = default)
        {
            T value = default;
            await Schedule(() =>
            {
                value = func();
            }, key, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return value;
        }

        public override async void Delete(string key)
        {
            await ApplyChanges(() => DeleteInternal(key), key);
        }

        protected abstract void DeleteInternal(string key);

        public override Task DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            return ApplyChangesAsync(() => DeleteInternal(key), key, cancellationToken);
        }

        public override bool Exists(string key)
        {
            return GetValue(() => ExistsInternal(key), key);
        }

        protected abstract bool ExistsInternal(string key);

        public override Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetValueAsync(() => ExistsInternal(key), key, cancellationToken);
        }

        public override byte[] GetBytes(string key)
        {
            return GetValue(() => GetBytesInternal(key), key);
        }

        protected abstract byte[] GetBytesInternal(string key);

        public override Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetValueAsync(() => GetBytesInternal(key), key, cancellationToken);
        }

        public override float GetFloat(string key)
        {
            return GetValue(() => GetFloatInternal(key), key);
        }

        protected abstract float GetFloatInternal(string key);

        public override Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetValueAsync(() => GetFloatInternal(key), key, cancellationToken);
        }

        public override int GetInt(string key)
        {
            return GetValue(() => GetIntInternal(key), key);
        }

        protected abstract int GetIntInternal(string key);

        public override Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetValueAsync(() => GetIntInternal(key), key, cancellationToken);
        }

        public override string GetString(string key)
        {
            return GetValue(() => GetStringInternal(key), key);
        }

        protected abstract string GetStringInternal(string key);

        public override Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
        {
            return GetValueAsync(() => GetStringInternal(key), key, cancellationToken);
        }

        public override async void SetBytes(string key, byte[] value)
        {
            await ApplyChanges(() => SetBytesInternal(key, value), key);
        }

        protected abstract void SetBytesInternal(string key, byte[] value);

        public override Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default)
        {
            return ApplyChangesAsync(() => SetBytesInternal(key, value), key, cancellationToken);
        }

        public override async void SetFloat(string key, float value)
        {
            await ApplyChanges(() => SetFloatInternal(key, value), key);
        }

        protected abstract void SetFloatInternal(string key, float value);

        public override Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default)
        {
            return ApplyChangesAsync(() => SetFloatInternal(key, value), key, cancellationToken);
        }

        public override async void SetInt(string key, int value)
        {
            await ApplyChanges(() => SetIntInternal(key, value), key);
        }

        protected abstract void SetIntInternal(string key, int value);

        public override Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default)
        {
            return ApplyChangesAsync(() => SetIntInternal(key, value), key, cancellationToken);
        }

        public override async void SetString(string key, string value)
        {
            await ApplyChanges(() => SetStringInternal(key, value), key);
        }

        protected abstract void SetStringInternal(string key, string value);

        public override Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            return ApplyChangesAsync(() => SetStringInternal(key, value), key, cancellationToken);
        }

        protected override string GetPathToken(IStorage storage)
        {
            if (storage == this || !(storage is BaseFileStorage))
            {
                return base.GetPathToken(storage);
            }
            else
            {
                return base.GetPathToken(storage) + "_Data";
            }
        }

        public override bool OpenStorageForDebug()
        {
            var process = Process.Start("explorer.exe", $"\"{Path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar)}\"");
            return true;
        }
    }
}