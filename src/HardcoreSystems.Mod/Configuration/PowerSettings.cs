namespace HardcoreSystems.Configuration
{
    public sealed class PowerSettings
    {
        public PowerSettings()
        {
            GeneratorRebalanceEnabled = false;
            SolarPanelGenerationHeatEnabled = false;
            OverloadHeatEnabled = false;
        }

        public bool GeneratorRebalanceEnabled { get; set; }
        public bool SolarPanelGenerationHeatEnabled { get; set; }
        public bool OverloadHeatEnabled { get; set; }
    }
}
