namespace HardcoreSystems.Configuration
{
    public sealed class FluidPressureSettings
    {
        public FluidPressureSettings()
        {
            Enabled = false;
            LiquidFillThreshold = 0.10f;
            SimulationIntervalSeconds = 600f;
        }

        public bool Enabled { get; set; }
        public float LiquidFillThreshold { get; set; }
        public float SimulationIntervalSeconds { get; set; }
    }
}
