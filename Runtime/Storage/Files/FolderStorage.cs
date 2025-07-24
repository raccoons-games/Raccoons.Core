using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage.Files
{
    /// <summary>
    /// Creates a folder and stores all the values in separate files in this folder
    /// </summary>
    public class FolderStorage : BaseFileSystemStorage
    {
        private Dictionary<string, Task> _taskDictionary = new Dictionary<string, Task>();

        public FolderStorage(string key, FileStorageSettings settings, IStorage parent = null) : base(key, settings, parent)
        {
            string path = Path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public FolderStorage(string key, IStorage parent = null) : this(key, new FileStorageSettings(), parent)
        {
        }

        protected override void DeleteInternal(string key)
        {
            File.Delete(PathOf(key));
        }

        protected override bool ExistsInternal(string key)
        {
            return File.Exists(PathOf(key));
        }

        protected override byte[] GetBytesInternal(string key)
        {
            return File.ReadAllBytes(PathOf(key));
        }

        protected override float GetFloatInternal(string key)
        {
            return float.Parse(File.ReadAllText(PathOf(key)));
        }

        protected override int GetIntInternal(string key)
        {
            return int.Parse(File.ReadAllText(PathOf(key)));
        }

        protected override string GetStringInternal(string key)
        {
            return File.ReadAllText(key);
        }

        protected override Task Schedule(Func<CancellationToken, Task> task, string key = null, CancellationToken token = default)
        {
            Task topTask = (_taskDictionary.ContainsKey(key)) ? _taskDictionary[key] : Task.CompletedTask;
            return _taskDictionary[key] = Task.Run(() => Schedule(task, topTask, token));
        }

        public async Task Schedule(Func<CancellationToken, Task> task, Task previosTask, CancellationToken token = default)
        {
            await previosTask;
            if (!token.IsCancellationRequested)
            {
                await task(token);
            }
        }

        protected override void SetBytesInternal(string key, byte[] value)
        {
            File.WriteAllBytes(PathOf(key), value);
        }

        protected override void SetFloatInternal(string key, float value)
        {
            File.WriteAllText(PathOf(key), value.ToString());
        }

        protected override void SetIntInternal(string key, int value)
        {
            File.WriteAllText(PathOf(key), value.ToString());
        }

        protected override void SetStringInternal(string key, string value)
        {
            File.WriteAllText(PathOf(key), value);
        }

        protected override void WaitForTopTask(string key = null)
        {
            if (_taskDictionary.ContainsKey(key))
            {
                _taskDictionary[key].Wait();
            }
        }
    }
}