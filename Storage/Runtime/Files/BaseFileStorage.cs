using Raccoons.Files;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage.Files
{
    public abstract class BaseFileStorage : BaseFileSystemStorage
    {
        private Task _topTask = Task.CompletedTask;
        private IFileReader _fileReader;
        private IFileWriter _fileWriter;
        protected BaseFileStorage(string key, FileStorageSettings settings, IStorage parent = null) : base(key, settings, parent)
        {
            if (Settings.AutoStore)
            {
                Load();
            }
        }

        protected BaseFileStorage(string key, IStorage parent = null) : this(key, new FileStorageSettings(), parent)
        {
        }

        protected BaseFileStorage(string key, IFileReader pathFileReader, IFileWriter pathFileWriter, FileStorageSettings settings,
            IStorage parent = null) : this(key, settings, parent)
        {
            FileReader = pathFileReader;
            FileWriter = pathFileWriter;
        }

        protected BaseFileStorage(string key, IFileReader pathFileReader, IFileWriter pathFileWriter, IStorage parent = null)
            : this(key, pathFileReader, pathFileWriter, new FileStorageSettings(), parent)
        {
        }

        public IFileReader FileReader { get => _fileReader ?? new PathFileReader(Path); set => _fileReader = value; }
        public IFileWriter FileWriter { get => _fileWriter ?? new PathFileWriter(Path); set => _fileWriter = value; }
        public bool IsDirty { get; private set; }
        public bool Loaded { get; protected set; } = false;

        protected override Task Schedule(System.Func<CancellationToken, Task> task, string key = null, CancellationToken token = default)
        {
            Task currentTopTask = _topTask;
            return _topTask = Task.Run(() => Schedule(task, currentTopTask, token), token).ContinueWith((task) =>
            {
                if (task.IsFaulted)
                {
                    throw task.Exception;
                }
            });
        }

        protected async Task Schedule(Func<CancellationToken, Task> task, Task previosTask, CancellationToken token = default)
        {
            await previosTask;
            token.ThrowIfCancellationRequested();
            await task(token);
        }


        protected override void WaitForTopTask(string key = null)
        {
            _topTask.Wait();
        }

        protected override async Task ApplyChanges(System.Action action, string key = null)
        {
            await base.ApplyChanges(action, key);
            SetDirty();
        }

        protected override async Task ApplyChangesAsync(System.Action action, string key = null, CancellationToken cancellationToken = default)
        {
            await base.ApplyChangesAsync(action, key, cancellationToken);
            await SetDirtyAsync();
        }

        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return GetValueAsync(() =>
            {
                SaveInternal();
                return true;
            }, null, cancellationToken);
        }

        protected void SaveInternal()
        {
            CheckSafeSave(out bool safeSave, out string path, out string tempPath, out IFileWriter pathFileWriter);
            using (var fileWriter = pathFileWriter.CreateStreamWriter())
            {
                SaveInternal(fileWriter);
            }
            if (safeSave)
            {
                File.Replace(tempPath, path, null);
            }
        }

        protected abstract void SaveInternal(StreamWriter fileWriter);

        protected void CheckSafeSave(out bool safeSave, out string path, out string tempPath, out IFileWriter currentFileWriter)
        {
            safeSave = Loaded && Settings.SafeSave && _fileWriter == null;
            IFileWriter fileWriter = FileWriter;
            if (fileWriter is PathFileWriter pathFileWriter)
            {
                path = pathFileWriter.FilePath;
            }
            else path = Path;
            if (safeSave)
            {
                tempPath = path + ".tmp";
                currentFileWriter = new PathFileWriter(tempPath);

            }
            else
            {
                tempPath = path;
                currentFileWriter = fileWriter;
            }
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return GetValueAsync(() =>
            {
                LoadInternal();
                return Task.FromResult(true);
            }, null, cancellationToken);
        }

        private void SetDirty()
        {
            IsDirty = true;
            if (Settings.AutoStore)
            {
                Save();
                IsDirty = false;
            }
        }

        private async Task SetDirtyAsync(CancellationToken cancellationToken = default)
        {
            IsDirty = true;
            if (Settings.AutoStore)
            {
                await SaveAsync(cancellationToken);
                IsDirty = false;
            }
        }

        public void SetDirtyAfter(System.Action action)
        {
            bool cachedAutoStore = Settings.AutoStore;
            Settings.AutoStore = false;
            action?.Invoke();
            Settings.AutoStore = cachedAutoStore;
            SetDirty();
        }

        public async Task SetDirtyAfterAsync(CancellationToken cancellationToken, params Task[] tasks)
        {
            bool cachedAutoStore = Settings.AutoStore;
            Settings.AutoStore = false;
            await Task.WhenAll(tasks);
            Settings.AutoStore = cachedAutoStore;
            cancellationToken.ThrowIfCancellationRequested();
            await SetDirtyAsync(cancellationToken);
        }

        public Task SetDirtyAfterAsync(params Task[] tasks) => SetDirtyAfterAsync(CancellationToken.None, tasks);

        public void Save()
        {
            GetValue(() =>
            {
                SaveInternal();
                return true;
            });
        }


        public void Load()
        {
            GetValue(() =>
            {
                LoadInternal();
                return true;
            });
        }

        protected void LoadInternal()
        {
            if (File.Exists(Path))
            {
                using (var fileReader = FileReader.CreateStreamReader())
                {
                    LoadInternal(fileReader);
                }
                Loaded = true;
            }
        }

        protected abstract void LoadInternal(StreamReader fileReader);
    }

    
}