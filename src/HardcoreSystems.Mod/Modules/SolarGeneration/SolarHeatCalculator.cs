using System;

namespace HardcoreSystems.Modules.SolarGeneration
{
    public static class SolarHeatCalculator
    {
        public static double CalculateSolarHeatDtuPerSecond(
            double actualGeneratedPowerWatts,
            double maximumPowerWatts,
            double maximumHeatDtuPerSecond)
        {
            if (double.IsNaN(actualGeneratedPowerWatts)
                || double.IsInfinity(actualGeneratedPowerWatts)
                || double.IsNaN(maximumPowerWatts)
                || double.IsInfinity(maximumPowerWatts)
                || double.IsNaN(maximumHeatDtuPerSecond)
                || double.IsInfinity(maximumHeatDtuPerSecond)
                || maximumPowerWatts <= 0.0
                || maximumHeatDtuPerSecond < 0.0)
            {
                return 0.0;
            }

            var ratio = actualGeneratedPowerWatts / maximumPowerWatts;
            ratio = Math.Max(0.0, Math.Min(1.0, ratio));
            return ratio * maximumHeatDtuPerSecond;
        }

        public static double CalculateEnergyDtu(double heatDtuPerSecond, double deltaTimeSeconds)
        {
            if (double.IsNaN(heatDtuPerSecond)
                || double.IsInfinity(heatDtuPerSecond)
                || double.IsNaN(deltaTimeSeconds)
                || double.IsInfinity(deltaTimeSeconds)
                || heatDtuPerSecond <= 0.0
                || deltaTimeSeconds <= 0.0)
            {
                return 0.0;
            }

            return heatDtuPerSecond * deltaTimeSeconds;
        }
    }
}
