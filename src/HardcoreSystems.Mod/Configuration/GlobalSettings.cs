namespace HardcoreSystems.Configuration
{
    public sealed class GlobalSettings
    {
        public GlobalSettings()
        {
            Language = "en";
            LogLevel = "Info";
            LastSelectedPreset = DifficultyPreset.Off;
        }

        public string Language { get; set; }
        public string LogLevel { get; set; }
        public DifficultyPreset LastSelectedPreset { get; set; }
    }
}
