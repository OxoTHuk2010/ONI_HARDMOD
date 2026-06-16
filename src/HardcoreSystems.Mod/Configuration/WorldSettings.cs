namespace HardcoreSystems.Configuration
{
    public sealed class WorldSettings
    {
        public WorldSettings()
        {
            Enabled = false;
            AsteroidSize = "Vanilla";
            IsReadOnlyAfterWorldCreation = true;
        }

        public bool Enabled { get; set; }
        public string AsteroidSize { get; set; }
        public bool IsReadOnlyAfterWorldCreation { get; set; }
    }
}
