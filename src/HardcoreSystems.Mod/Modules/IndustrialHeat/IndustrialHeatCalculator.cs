using System;

namespace HardcoreSystems.Modules.IndustrialHeat
{
    public static class IndustrialHeatCalculator
    {
        private const float PumpWattsPerKilowattHeat = 120f;

        public static float CalculateTemperatureDelta(
            float wattsUsed,
            float heatMultiplier,
            float dt,
            float mass,
            float specificHeatCapacity,
            float maxDeltaKelvin)
        {
            if (wattsUsed <= 0f || heatMultiplier <= 0f || dt <= 0f || mass <= 0f || specificHeatCapacity <= 0f)
            {
                return 0f;
            }

            var addedKilojoules = wattsUsed * heatMultiplier * dt * 0.001f;
            var delta = addedKilojoules / (mass * specificHeatCapacity);
            if (float.IsNaN(delta) || float.IsInfinity(delta) || delta <= 0f)
            {
                return 0f;
            }

            return Math.Min(delta, maxDeltaKelvin);
        }

        public static float ScaleOperatingKilowatts(float kilowatts, float heatMultiplier)
        {
            if (kilowatts <= 0f || heatMultiplier <= 0f)
            {
                return kilowatts;
            }

            var scaled = kilowatts * heatMultiplier;
            if (float.IsNaN(scaled) || float.IsInfinity(scaled))
            {
                return kilowatts;
            }

            return scaled;
        }

        public static float CalculatePumpFallbackKilowatts(float energyConsumptionWhenActive)
        {
            if (energyConsumptionWhenActive <= 0f)
            {
                return 0f;
            }

            return energyConsumptionWhenActive / PumpWattsPerKilowattHeat;
        }
    }
}
