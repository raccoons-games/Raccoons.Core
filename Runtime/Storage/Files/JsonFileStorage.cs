using Raccoons.Files;
using Raccoons.Serialization.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Raccoons.Storage.Files
{
    /// <summary>
    /// Creates a json-file and stores all the values in it. You can also override file Reader/Writer with your custom (writer doesn't support safe save)
    /// </summary>
    public class JsonFileStorage : BaseFileStorage, IEnumerable<KeyValuePair<string, object>>
    {
        private NewtonsoftJsonDictionary _dictionary = new NewtonsoftJsonDictionary();
        private object _dictionaryLock = new object();

        public JsonFileStorage(string key, FileStorageSettings settings, IStorage parent = null) : base(key, settings, parent)
        {
            
        }

        public JsonFileStorage(string key, IStorage parent = null) : base(key, parent)
        {

        }

        public JsonFileStorage(string key, IFileReader fileReader, IFileWriter fileWriter, FileStorageSettings settings,
            IStorage parent = null) : base(key, fileReader, fileWriter, settings, parent)
        {
        }

        public JsonFileStorage(string key, IFileReader pathFileReader, IFileWriter pathFileWriter, IStorage parent = null)
            : base(key, pathFileReader, pathFileWriter, parent)
        {
        }

        protected override void LoadInternal(StreamReader fileReader)
        {
            lock (_dictionaryLock)
            {
                _dictionary.Load(fileReader);
            }
        }

        protected override void SaveInternal(StreamWriter fileWriter)
        {
            lock (_dictionaryLock)
            {
                _dictionary.Save(fileWriter);
            }
        }


        protected override void DeleteInternal(string key)
        {
            lock (_dictionaryLock)
            {
                _dictionary.Remove(key);
            }
        }

        protected override bool ExistsInternal(string key)
        {
            lock (_dictionaryLock)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        protected override byte[] GetBytesInternal(string key)
        {
            lock (_dictionaryLock)
            {
                return (byte[])_dictionary[key];
            }
        }

        protected override float GetFloatInternal(string key)
        {
            lock (_dictionaryLock)
            {
                return (float)_dictionary[key];
            }
        }

        protected override int GetIntInternal(string key)
        {
            lock (_dictionaryLock)
            {
                return (int)_dictionary[key];
            }
        }

        protected override string GetStringInternal(string key)
        {
            lock (_dictionaryLock)
            {
                return (string)_dictionary[key];
            }
        }

        protected override void SetBytesInternal(string key, byte[] value)
        {
            lock (_dictionaryLock)
            {
                _dictionary[key] = value;
            }
        }

        protected override void SetFloatInternal(string key, float value)
        {
            lock (_dictionaryLock)
            {
                _dictionary[key] = value;
            }
        }

        protected override void SetIntInternal(string key, int value)
        {
            lock (_dictionaryLock)
            {
                _dictionary[key] = value;
            }
        }

        protected override void SetStringInternal(string key, string value)
        {
            lock (_dictionaryLock)
            {
                _dictionary[key] = value;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            lock (_dictionaryLock)
            {
                return ((IEnumerable<KeyValuePair<string, object>>)_dictionary).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_dictionaryLock)
            {
                return ((IEnumerable)_dictionary).GetEnumerator();
            }
        }
    }
}
