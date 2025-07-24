using Raccoons.Serialization.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Raccoons.Storage
{
    /// <summary>
    /// Storage that stores all values as json-string value in parent storage
    /// </summary>
    public class JsonInternalStorage : BaseInternalStorage
    {
        private NewtonsoftJsonDictionary _dictionary = new NewtonsoftJsonDictionary();
        public JsonInternalStorage(string key, IStorage parent) : base(key, parent)
        {
        }

        public override bool Exists(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public override Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dictionary.ContainsKey(key));
        }

        public override byte[] GetBytes(string key)
        {
            return (byte[])_dictionary[key];
        }

        public override float GetFloat(string key)
        {
            return (float)_dictionary[key];
        }

        public override int GetInt(string key)
        {
            return (int)_dictionary[key];
        }

        public override string GetString(string key)
        {
            return (string)_dictionary[key];
        }

        protected override void DeleteInternal(string key)
        {
            _dictionary.Remove(key);
        }

        protected override void Load(string storageString)
        {
            using (StringReader streamReader = new StringReader(storageString))
            {
                _dictionary.Load(streamReader);
            }
        }

        protected override void SetBytesInternal(string key, byte[] value)
        {
            _dictionary[key] = value;
        }

        protected override void SetFloatInternal(string key, float value)
        {
            _dictionary[key] = value;
        }

        protected override void SetIntInternal(string key, int value)
        {
            _dictionary[key] = value;
        }

        protected override void SetStringInternal(string key, string value)
        {
            _dictionary[key] = value;
        }

        protected override string StringifyStorage()
        {
            string result;
            using (StringWriter stringWriter = new StringWriter())
            {
                _dictionary.Save(stringWriter);
                result = stringWriter.ToString();
            }
            return result;

        }
    }
}