namespace HardcoreSystems.Modules.GeneratorEfficiency
{
    public static class GeneratorEfficiencyCalculator
    {
        public static float Apply(float vanillaWatts, float efficiency)
        {
            if (vanillaWatts <= 0f)
            {
                return vanillaWatts;
            }

            if (efficiency < 0.10f)
            {
                efficiency = 0.10f;
            }
            else if (efficiency > 1f)
            {
                efficiency = 1f;
            }

            return vanillaWatts * efficiency;
        }
    }
}
