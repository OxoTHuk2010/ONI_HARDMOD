namespace HardcoreSystems.Configuration
{
    public sealed class WorldSettings
    {
        public WorldSettings()
        {
            Enabled = false;
            AsteroidSize = "Vanilla";
            TemperatureOffsetCelsius = 0f;
            TemperatureMultiplier = 1f;
            IsReadOnlyAfterWorldCreation = true;
        }

        public bool Enabled { get; set; }
        public string AsteroidSize { get; set; }
        public float TemperatureOffsetCelsius { get; set; }
        public float TemperatureMultiplier { get; set; }
        public bool IsReadOnlyAfterWorldCreation { get; set; }
    }
}
