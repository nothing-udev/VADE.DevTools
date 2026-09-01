namespace VADE.DevTools.Persistence
{

    public interface IAutoSaveStorage
    {
        bool HasKey(string key);
        void Write(string key, string serialized);
        string Read(string key);
        void Delete(string key);
    }
}
