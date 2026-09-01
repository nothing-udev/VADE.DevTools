namespace VADE.DevTools.Persistence
{

    public static class AutoSaveSerializer
    {
        public static IAutoSaveSerializer Current { get; set; } = new NewtonsoftAutoSaveSerializer();
    }
}
