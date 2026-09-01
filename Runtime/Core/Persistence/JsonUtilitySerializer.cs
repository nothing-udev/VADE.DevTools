using System;
using UnityEngine;

namespace VADE.DevTools.Persistence
{

    public class JsonUtilitySerializer : IAutoSaveSerializer
    {
        [Serializable]
        private class Wrapper<TValue>
        {
            public TValue value;
        }

        public string Serialize<T>(T value)
        {
            return JsonUtility.ToJson(new Wrapper<T> { value = value });
        }

        public T Deserialize<T>(string serialized)
        {
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(serialized);
            return wrapper != null ? wrapper.value : default;
        }
    }
}
