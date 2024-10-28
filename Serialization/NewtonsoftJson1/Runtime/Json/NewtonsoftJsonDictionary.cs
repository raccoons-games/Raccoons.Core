using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raccoons.Serialization.Json
{
    /// <summary>
    /// Dictionary of json value with save/load methods for text reader
    /// </summary>
    public class NewtonsoftJsonDictionary : IDictionary<string, object>
    {
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public object this[string key] { get => ((IDictionary<string, object>)_dictionary)[key]; set => ((IDictionary<string, object>)_dictionary)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, object>)_dictionary).Keys;

        public ICollection<object> Values => ((IDictionary<string, object>)_dictionary).Values;

        public int Count => ((ICollection<KeyValuePair<string, object>>)_dictionary).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, object>>)_dictionary).IsReadOnly;

        public void Add(string key, object value)
        {
            ((IDictionary<string, object>)_dictionary).Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)_dictionary).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, object>>)_dictionary).Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_dictionary).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, object>)_dictionary).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)_dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)_dictionary).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, object>)_dictionary).Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_dictionary).Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return ((IDictionary<string, object>)_dictionary).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dictionary).GetEnumerator();
        }

        public void Load(TextReader textReader)
        {
            _dictionary.Clear();
            using (var jsonReader = new JsonTextReader(textReader))
            {
                string propertyName = "";
                while (jsonReader.Read())
                {
                    ParseToken(jsonReader, ref propertyName);
                }
            }
        }

        private void ParseToken(JsonTextReader jsonReader, ref string propertyName)
        {
            switch (jsonReader.TokenType)
            {
                case JsonToken.PropertyName:
                    propertyName = (string)jsonReader.Value;
                    break;
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Bytes:
                    _dictionary[propertyName] = jsonReader.Value;
                    break;
            }
        }

        public void Save(TextWriter textWriter)
        {
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                jsonWriter.WriteStartObject();
                foreach (var pair in _dictionary)
                {
                    jsonWriter.WritePropertyName(pair.Key);
                    jsonWriter.WriteValue(pair.Value);
                }
                jsonWriter.WriteEndObject();
            }
        }
    }
}
