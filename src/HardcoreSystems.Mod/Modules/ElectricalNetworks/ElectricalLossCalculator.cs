using System;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    public static class ElectricalLossCalculator
    {
        private const float BaseCircuitLossRatio = 0.02f;
        private const float WireHeatShare = 0.05f;
        private const float OverloadHeatRatio = 0.01f;

        public static float CalculateCircuitLossWatts(float wattsUsed, float maxSafeWatts, float resistanceMultiplier)
        {
            if (wattsUsed <= 0f || maxSafeWatts <= 0f || resistanceMultiplier <= 0f)
            {
                return 0f;
            }

            var loadRatio = wattsUsed / maxSafeWatts;
            var loss = wattsUsed * BaseCircuitLossRatio * resistanceMultiplier * loadRatio * loadRatio;
            if (float.IsNaN(loss) || float.IsInfinity(loss) || loss <= 0f)
            {
                return 0f;
            }

            return loss;
        }

        public static float CalculateWireDissipationWatts(float circuitLossWatts)
        {
            if (circuitLossWatts <= 0f)
            {
                return 0f;
            }

            return Math.Min(circuitLossWatts * WireHeatShare, 20f);
        }

        public static float CalculateOverloadHeatWatts(float wattsUsed, float maxSafeWatts, float resistanceMultiplier)
        {
            if (wattsUsed <= 0f || maxSafeWatts <= 0f || wattsUsed <= maxSafeWatts)
            {
                return 0f;
            }

            var overloadWatts = wattsUsed - maxSafeWatts;
            var heat = overloadWatts * OverloadHeatRatio * (1f + Math.Max(0f, resistanceMultiplier));
            if (float.IsNaN(heat) || float.IsInfinity(heat) || heat <= 0f)
            {
                return 0f;
            }

            return Math.Min(heat, 200f);
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
