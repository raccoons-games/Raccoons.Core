using Newtonsoft.Json;

namespace Raccoons.Serialization.Json
{
    public class NewtonsoftJsonSerializer : ISerializer
    {
        public T Deserialize<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

    }
}
