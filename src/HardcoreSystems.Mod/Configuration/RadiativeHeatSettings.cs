namespace HardcoreSystems.Configuration
{
    public sealed class RadiativeHeatSettings
    {
        public RadiativeHeatSettings()
        {
            Enabled = false;
            SimulationIntervalSeconds = 1f;
            RadiationFactor = 0f;
        }

        public bool Enabled { get; set; }
        public float SimulationIntervalSeconds { get; set; }
        public float RadiationFactor { get; set; }
    }
}
