namespace HardcoreSystems.Modules.DuplicantBalance
{
    public static class DuplicantBalanceCalculator
    {
        public static float ApplyExperienceMultiplier(float vanillaExperience, float multiplier)
        {
            return ApplyPositiveMultiplier(vanillaExperience, multiplier, 0.05f, 10f);
        }

        public static float ApplyCaloriesMultiplier(float vanillaCalories, float multiplier)
        {
            return ApplySignedMultiplier(vanillaCalories, multiplier, 0.10f, 10f);
        }

        public static float CalculateMissingCalorieDelta(float currentDelta, float vanillaDelta, float multiplier)
        {
            var targetDelta = ApplyCaloriesMultiplier(vanillaDelta, multiplier);
            if (System.Math.Abs(currentDelta) >= System.Math.Abs(targetDelta) - 0.0001f)
            {
                return 0f;
            }

            return targetDelta - currentDelta;
        }

        private static float ApplyPositiveMultiplier(float value, float multiplier, float min, float max)
        {
            if (value <= 0f)
            {
                return value;
            }

            if (multiplier < min)
            {
                multiplier = min;
            }
            else if (multiplier > max)
            {
                multiplier = max;
            }

            return value * multiplier;
        }

        private static float ApplySignedMultiplier(float value, float multiplier, float min, float max)
        {
            if (value == 0f)
            {
                return value;
            }

            if (multiplier < min)
            {
                multiplier = min;
            }
            else if (multiplier > max)
            {
                multiplier = max;
            }

            return value * multiplier;
        }
    }
}
