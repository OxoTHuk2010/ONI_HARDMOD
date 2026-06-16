using System;

namespace HardcoreSystems.Modules.PowerGeneration
{
    public static class PowerGenerationHeatCalculator
    {
        public static double CalculateProfileHeatDtu(double bodyHeatKilowatts, double deltaTimeSeconds)
        {
            if (double.IsNaN(bodyHeatKilowatts)
                || double.IsInfinity(bodyHeatKilowatts)
                || double.IsNaN(deltaTimeSeconds)
                || double.IsInfinity(deltaTimeSeconds)
                || bodyHeatKilowatts <= 0.0
                || deltaTimeSeconds <= 0.0)
            {
                return 0.0;
            }

            return bodyHeatKilowatts * 1000.0 * deltaTimeSeconds;
        }

        public static double CalculateFuelSurplusHeatDtu(
            double consumedMassKilograms,
            double specificHeatCapacity,
            double fuelTemperatureKelvin,
            double bodyTemperatureKelvin)
        {
            if (double.IsNaN(consumedMassKilograms)
                || double.IsInfinity(consumedMassKilograms)
                || double.IsNaN(specificHeatCapacity)
                || double.IsInfinity(specificHeatCapacity)
                || double.IsNaN(fuelTemperatureKelvin)
                || double.IsInfinity(fuelTemperatureKelvin)
                || double.IsNaN(bodyTemperatureKelvin)
                || double.IsInfinity(bodyTemperatureKelvin)
                || consumedMassKilograms <= 0.0
                || specificHeatCapacity <= 0.0)
            {
                return 0.0;
            }

            var delta = Math.Max(0.0, fuelTemperatureKelvin - bodyTemperatureKelvin);
            return consumedMassKilograms * 1000.0 * specificHeatCapacity * delta;
        }
    }
}
