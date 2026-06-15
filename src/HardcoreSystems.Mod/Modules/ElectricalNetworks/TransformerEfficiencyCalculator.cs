using System;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    public static class TransformerEfficiencyCalculator
    {
        public static float CalculateAdditionalInputJoules(float outputJoules, float efficiency)
        {
            if (outputJoules <= 0f)
            {
                return 0f;
            }

            var clampedEfficiency = Math.Max(0.10f, Math.Min(1f, efficiency));
            var inputJoules = outputJoules / clampedEfficiency;
            var extra = inputJoules - outputJoules;
            if (float.IsNaN(extra) || float.IsInfinity(extra) || extra <= 0f)
            {
                return 0f;
            }

            return extra;
        }

        public static float CalculateInputJoules(float outputJoules, float efficiency)
        {
            if (outputJoules <= 0f)
            {
                return 0f;
            }

            return outputJoules + CalculateAdditionalInputJoules(outputJoules, efficiency);
        }
    }
}
