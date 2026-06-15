namespace HardcoreSystems.Configuration
{
    public sealed class PowerSettings
    {
        public PowerSettings()
        {
            GeneratorEfficiencyEnabled = false;
            ElectricalLossesEnabled = false;
            TransformerEfficiencyEnabled = false;
            OverloadHeatEnabled = false;
            GeneratorEfficiency = 1f;
            TransformerEfficiency = 1f;
            WireResistanceMultiplier = 0f;
        }

        public bool GeneratorEfficiencyEnabled { get; set; }
        public bool ElectricalLossesEnabled { get; set; }
        public bool TransformerEfficiencyEnabled { get; set; }
        public bool OverloadHeatEnabled { get; set; }
        public float GeneratorEfficiency { get; set; }
        public float TransformerEfficiency { get; set; }
        public float WireResistanceMultiplier { get; set; }
    }
}
