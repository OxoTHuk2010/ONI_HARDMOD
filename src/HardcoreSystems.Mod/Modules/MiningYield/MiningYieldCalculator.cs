namespace HardcoreSystems.Modules.MiningYield
{
    public static class MiningYieldCalculator
    {
        public static float Apply(float vanillaMass, float yieldMultiplier)
        {
            if (vanillaMass <= 0f)
            {
                return vanillaMass;
            }

            if (yieldMultiplier >= 0.9999f)
            {
                return vanillaMass;
            }

            return vanillaMass * Clamp(yieldMultiplier, 0.01f, 1f);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}
