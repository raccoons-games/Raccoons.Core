using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage
{
    public abstract class BaseStorage : IStorage
    {
        private readonly string _key;
        protected List<IStorage> _children = new List<IStorage>();
        private IStorage _parent;

        protected BaseStorage(string key, IStorage parent)
        {
            _key = key;
            Parent = parent;
        }

        public string Key => _key;
        public string Path { get => GetPath(); }
        public abstract char Separator { get; }
        public IEnumerable<IStorage> Children => _children;

        public virtual IStorage Parent
        {
            get => _parent;
            set
            {
                if (Parent != null)
                {
                    Parent.RemoveChild(this);
                }
                _parent = value;
                if (Parent != null)
                {
                    Parent.AddChild(this);
                }
            }
        }

        private string[] GetPathArray()
        {
            Stack<string> path = new Stack<string>();
            IStorage current = this;
            while (current != null)
            {
                path.Push(GetPathToken(current));
                current = current.Parent;
            }
            return path.ToArray();
        }

        protected virtual string GetPathToken(IStorage storage)
        {
            return storage.Key;
        }

        protected virtual string GetPath()
        {
            return JoinPath(GetPathArray());
        }

        public string JoinPath(params string[] path)
        {
            return string.Join(Separator, path);
        }

        public string PathOf(string key)
        {
            return JoinPath(Path, key);
        }

        void IStorage.AddChild(IStorage child)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
            }
        }

        void IStorage.RemoveChild(IStorage child)
        {
            _children.Remove(child);
        }

        public abstract void Delete(string key);
        public abstract Task DeleteAsync(string key, CancellationToken cancellationToken = default);
        public abstract bool Exists(string key);
        public abstract Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
        public abstract byte[] GetBytes(string key);
        public abstract Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default);
        public abstract float GetFloat(string key);
        public abstract Task<float> GetFloatAsync(string key, CancellationToken cancellationToken = default);
        public abstract int GetInt(string key);
        public abstract Task<int> GetIntAsync(string key, CancellationToken cancellationToken = default);
        public abstract string GetString(string key);
        public abstract Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default);
        public abstract void SetBytes(string key, byte[] value);
        public abstract Task SetBytesAsync(string key, byte[] value, CancellationToken cancellationToken = default);
        public abstract void SetFloat(string key, float value);
        public abstract Task SetFloatAsync(string key, float value, CancellationToken cancellationToken = default);
        public abstract void SetInt(string key, int value);
        public abstract Task SetIntAsync(string key, int value, CancellationToken cancellationToken = default);
        public abstract void SetString(string key, string value);
        public abstract Task SetStringAsync(string key, string value, CancellationToken cancellationToken = default);

        public virtual bool OpenStorageForDebug()
        {
            return false;
        }
    }
}