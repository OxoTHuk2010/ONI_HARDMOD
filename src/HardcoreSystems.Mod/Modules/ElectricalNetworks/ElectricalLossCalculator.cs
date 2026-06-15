using System;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    public static class ElectricalLossCalculator
    {
        public const float NominalVoltage = 1000f;
        private const float BaseOhmsPerCell = 0.002f;
        private const float TemperatureCoefficient = 0.004f;
        private const float ReferenceTemperatureKelvin = 293.15f;

        public static float CalculateCellResistanceOhms(
            float resistanceMultiplier,
            float materialFactor,
            float temperatureKelvin,
            float capacityWatts)
        {
            if (resistanceMultiplier <= 0f || materialFactor <= 0f || capacityWatts <= 0f)
            {
                return 0f;
            }

            var capacityFactor = Math.Max(0.20f, Math.Min(1f, 1000f / capacityWatts));
            var temperatureFactor = 1f + TemperatureCoefficient * (temperatureKelvin - ReferenceTemperatureKelvin);
            temperatureFactor = Math.Max(0.25f, Math.Min(4f, temperatureFactor));

            var resistance = BaseOhmsPerCell * resistanceMultiplier * materialFactor * temperatureFactor * capacityFactor;
            if (float.IsNaN(resistance) || float.IsInfinity(resistance) || resistance <= 0f)
            {
                return 0f;
            }

            return resistance;
        }

        public static float CalculateOhmicLossWatts(float loadWatts, float pathResistanceOhms)
        {
            if (loadWatts <= 0f || pathResistanceOhms <= 0f)
            {
                return 0f;
            }

            var current = loadWatts / NominalVoltage;
            var loss = current * current * pathResistanceOhms * NominalVoltage;
            if (float.IsNaN(loss) || float.IsInfinity(loss) || loss <= 0f)
            {
                return 0f;
            }

            return loss;
        }

        public static float CalculateAvailability(float requiredWatts, float lossWatts)
        {
            if (requiredWatts <= 0f)
            {
                return 1f;
            }

            var delivered = Math.Max(0f, requiredWatts - Math.Max(0f, lossWatts));
            var ratio = delivered / requiredWatts;
            if (float.IsNaN(ratio) || float.IsInfinity(ratio))
            {
                return 1f;
            }

            return Math.Max(0f, Math.Min(1f, ratio));
        }

        public static bool ShouldPowerConsumer(float availability, int phase, int slots)
        {
            if (availability >= 0.999f)
            {
                return true;
            }

            if (availability < 0.50f || slots <= 0)
            {
                return false;
            }

            var poweredSlots = (int)Math.Round(availability * slots);
            poweredSlots = Math.Max(1, Math.Min(slots, poweredSlots));
            var slot = Math.Abs(phase % slots);
            return slot < poweredSlots;
        }

        public static float CalculateOverloadHeatWatts(float wattsUsed, float maxSafeWatts, float resistanceMultiplier)
        {
            if (wattsUsed <= 0f || maxSafeWatts <= 0f || wattsUsed <= maxSafeWatts)
            {
                return 0f;
            }

            var overloadWatts = wattsUsed - maxSafeWatts;
            var heat = overloadWatts * (0.20f + 0.10f * Math.Max(0f, resistanceMultiplier));
            if (float.IsNaN(heat) || float.IsInfinity(heat) || heat <= 0f)
            {
                return 0f;
            }

            return Math.Min(heat, 10000f);
        }

        public static float CalculateShortCircuitImpulseWatts(int damage)
        {
            var damageFactor = Math.Max(1, damage);
            return Math.Min(2000000f, 500000f + damageFactor * 25000f);
        }

        public static float CalculateTemperatureDelta(
            float heatWatts,
            float dt,
            float mass,
            float specificHeatCapacity,
            float maxDeltaKelvin)
        {
            if (heatWatts <= 0f || dt <= 0f || mass <= 0f || specificHeatCapacity <= 0f || maxDeltaKelvin <= 0f)
            {
                return 0f;
            }

            var addedKilojoules = heatWatts * dt * 0.001f;
            var delta = addedKilojoules / (mass * specificHeatCapacity);
            if (float.IsNaN(delta) || float.IsInfinity(delta) || delta <= 0f)
            {
                return 0f;
            }

            return Math.Min(delta, maxDeltaKelvin);
        }
    }
}
