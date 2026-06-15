namespace HardcoreSystems.Configuration
{
    public sealed class DuplicantSettings
    {
        public DuplicantSettings()
        {
            Enabled = false;
            ExperienceMultiplier = 1f;
            CaloriesMultiplier = 1f;
        }

        public bool Enabled { get; set; }
        public float ExperienceMultiplier { get; set; }
        public float CaloriesMultiplier { get; set; }
    }
}
