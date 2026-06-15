namespace HardcoreSystems.Modules.DiseaseEffects
{
    public static class DiseasePenaltyCalculator
    {
        public static float ProductivityMultiplier(float severity)
        {
            return -ClampSeverity(severity);
        }

        public static int SlimelungMoralePenalty(float severity)
        {
            severity = ClampSeverity(severity);
            if (severity >= 0.75f)
            {
                return -8;
            }

            if (severity >= 0.45f)
            {
                return -5;
            }

            if (severity >= 0.25f)
            {
                return -3;
            }

            return severity > 0f ? -1 : 0;
        }

        public static float StressMultiplier(float severity)
        {
            return ClampSeverity(severity);
        }

        private static float ClampSeverity(float severity)
        {
            if (severity < 0f)
            {
                return 0f;
            }

            return severity > 0.95f ? 0.95f : severity;
        }
    }
}
