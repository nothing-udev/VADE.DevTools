namespace VADE.DevTools.Persistence
{

    public static class AutoSaveStorage
    {
        public static IAutoSaveStorage PlayerPrefsStorage { get; set; } = new PlayerPrefsAutoSaveStorage();
        public static IAutoSaveStorage FileStorage { get; set; } = new FileAutoSaveStorage();

        public static IAutoSaveStorage Get(AutoSaveType type) => type switch
        {
            AutoSaveType.File => FileStorage,
            _ => PlayerPrefsStorage
        };
    }
}
