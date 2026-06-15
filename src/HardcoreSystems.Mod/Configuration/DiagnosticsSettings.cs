namespace HardcoreSystems.Configuration
{
    public sealed class DiagnosticsSettings
    {
        public DiagnosticsSettings()
        {
            Enabled = false;
            ReportIntervalSeconds = 60f;
        }

        public bool Enabled { get; set; }
        public float ReportIntervalSeconds { get; set; }
    }
}
