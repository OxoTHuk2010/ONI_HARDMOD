using HardcoreSystems.Persistence;

namespace HardcoreSystems.Configuration
{
    public sealed class ModSettings
    {
        public ModSettings()
        {
            SchemaVersion = ConfigVersion.Current;
            Preset = DifficultyPreset.Off;
            Global = new GlobalSettings();
            World = new WorldSettings();
            Mining = new MiningSettings();
            RadiativeHeat = new RadiativeHeatSettings();
            Power = new PowerSettings();
            IndustrialHeat = new IndustrialHeatSettings();
            FluidPressure = new FluidPressureSettings();
            Diseases = new DiseaseSettings();
            Duplicants = new DuplicantSettings();
            Diagnostics = new DiagnosticsSettings();
        }

        public int SchemaVersion { get; set; }
        public DifficultyPreset Preset { get; set; }
        public GlobalSettings Global { get; set; }
        public WorldSettings World { get; set; }
        public MiningSettings Mining { get; set; }
        public RadiativeHeatSettings RadiativeHeat { get; set; }
        public PowerSettings Power { get; set; }
        public IndustrialHeatSettings IndustrialHeat { get; set; }
        public FluidPressureSettings FluidPressure { get; set; }
        public DiseaseSettings Diseases { get; set; }
        public DuplicantSettings Duplicants { get; set; }
        public DiagnosticsSettings Diagnostics { get; set; }
    }
}
