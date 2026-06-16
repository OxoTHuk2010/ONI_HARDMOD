namespace HardcoreSystems.Modules.SolarGeneration
{
    public sealed class SolarPanelProfile
    {
        public SolarPanelProfile(double maximumPowerWatts, double maximumHeatDtuPerSecond)
        {
            MaximumPowerWatts = maximumPowerWatts;
            MaximumHeatDtuPerSecond = maximumHeatDtuPerSecond;
        }

        public double MaximumPowerWatts { get; private set; }
        public double MaximumHeatDtuPerSecond { get; private set; }

        public static SolarPanelProfile Vanilla()
        {
            return new SolarPanelProfile(380.0, 5000.0);
        }
    }
}
