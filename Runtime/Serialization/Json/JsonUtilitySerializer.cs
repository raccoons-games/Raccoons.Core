using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Raccoons.Serialization.Json
{
    public class JsonUtilitySerializer : ISerializer
    {
        public T Deserialize<T>(string str)
        {
            return JsonUtility.FromJson<T>(str);
        }

        public string Serialize(object obj)
        {
            return JsonUtility.ToJson(obj);
        }
    }
}
