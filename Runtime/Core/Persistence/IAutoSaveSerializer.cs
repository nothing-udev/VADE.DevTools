namespace VADE.DevTools.Persistence
{
    public interface IAutoSaveSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string serialized);
    }
}
