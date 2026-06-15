namespace HardcoreSystems.Persistence
{
    public sealed class WorldGenerationSnapshot
    {
        public string AsteroidSize { get; set; }
        public float TemperatureOffsetCelsius { get; set; }
        public float TemperatureMultiplier { get; set; }
        public int SchemaVersion { get; set; }
    }
}
