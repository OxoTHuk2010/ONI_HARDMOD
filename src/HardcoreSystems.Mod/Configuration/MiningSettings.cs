namespace HardcoreSystems.Configuration
{
    public sealed class MiningSettings
    {
        public MiningSettings()
        {
            Enabled = false;
            YieldMultiplier = 1f;
        }

        public bool Enabled { get; set; }
        public float YieldMultiplier { get; set; }
    }
}
