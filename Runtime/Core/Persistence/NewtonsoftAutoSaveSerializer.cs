using Newtonsoft.Json;

namespace VADE.DevTools.Persistence
{

    public class NewtonsoftAutoSaveSerializer : IAutoSaveSerializer
    {
        public string Serialize<T>(T value) => JsonConvert.SerializeObject(value);

        public T Deserialize<T>(string serialized) => JsonConvert.DeserializeObject<T>(serialized);
    }
}
