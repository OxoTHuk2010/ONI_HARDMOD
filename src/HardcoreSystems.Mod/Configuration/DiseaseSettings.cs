namespace HardcoreSystems.Configuration
{
    public sealed class DiseaseSettings
    {
        public DiseaseSettings()
        {
            Enabled = false;
            SeverityMultiplier = 0f;
        }

        public bool Enabled { get; set; }
        public float SeverityMultiplier { get; set; }
    }
}
