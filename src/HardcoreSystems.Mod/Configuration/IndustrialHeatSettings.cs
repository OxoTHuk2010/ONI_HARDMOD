namespace HardcoreSystems.Configuration
{
    public sealed class IndustrialHeatSettings
    {
        public IndustrialHeatSettings()
        {
            Enabled = false;
            HeatMultiplier = 1f;
        }

        public bool Enabled { get; set; }
        public float HeatMultiplier { get; set; }
    }
}
