namespace HardcoreSystems.Configuration
{
    public sealed class PowerSettings
    {
        public PowerSettings()
        {
            GeneratorRebalanceEnabled = false;
            FuelThermalAccountingEnabled = false;
            SolarPanelGenerationHeatEnabled = false;
            ElectricalDiagnosticsEnabled = false;
            OverloadHeatMode = "ImmediateTarget";
            GeneratorEfficiencyEnabled = false;
            ElectricalLossesEnabled = false;
            TransformerEfficiencyEnabled = false;
            OverloadHeatEnabled = false;
            GeneratorEfficiency = 1f;
            TransformerEfficiency = 1f;
            WireResistanceMultiplier = 0f;
        }

        public bool GeneratorRebalanceEnabled { get; set; }
        public bool FuelThermalAccountingEnabled { get; set; }
        public bool SolarPanelGenerationHeatEnabled { get; set; }
        public bool ElectricalDiagnosticsEnabled { get; set; }
        public string OverloadHeatMode { get; set; }
        public bool GeneratorEfficiencyEnabled { get; set; }
        public bool ElectricalLossesEnabled { get; set; }
        public bool TransformerEfficiencyEnabled { get; set; }
        public bool OverloadHeatEnabled { get; set; }
        public float GeneratorEfficiency { get; set; }
        public float TransformerEfficiency { get; set; }
        public float WireResistanceMultiplier { get; set; }
    }
}
