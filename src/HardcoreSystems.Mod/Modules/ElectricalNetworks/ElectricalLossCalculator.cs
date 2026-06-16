using System;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    public static class ElectricalLossCalculator
    {
        private const float KelvinOffset = 273.15f;
        private const float TransitionSafetyRatio = 0.90f;
        private const float MinimumTransitionMarginKelvin = 5f;

        public static float CalculateSafeEmergencyTemperature(float highTempKelvin)
        {
            if (float.IsNaN(highTempKelvin) || float.IsInfinity(highTempKelvin) || highTempKelvin <= KelvinOffset)
            {
                return 0f;
            }

            var highTempCelsius = highTempKelvin - KelvinOffset;
            var targetKelvin = KelvinOffset + highTempCelsius * TransitionSafetyRatio;
            var cappedKelvin = Math.Min(targetKelvin, highTempKelvin - MinimumTransitionMarginKelvin);
            return Math.Max(KelvinOffset, cappedKelvin);
        }

        public static bool ShouldRaiseTemperature(float currentKelvin, float targetKelvin)
        {
            return targetKelvin > KelvinOffset && currentKelvin < targetKelvin;
        }
    }
}
