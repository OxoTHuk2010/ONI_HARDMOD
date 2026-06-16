using System;

namespace HardcoreSystems.Modules.ElectricalOverloadThermalDamage
{
    public static class OverloadHeatCalculator
    {
        private const double TargetTemperatureRatio = 0.90;

        public static OverloadHeatResult CalculateOverloadHeat(
            double currentTemperatureKelvin,
            double meltingTemperatureKelvin,
            double massGrams,
            double specificHeatCapacity)
        {
            if (double.IsNaN(currentTemperatureKelvin)
                || double.IsInfinity(currentTemperatureKelvin)
                || double.IsNaN(meltingTemperatureKelvin)
                || double.IsInfinity(meltingTemperatureKelvin)
                || double.IsNaN(massGrams)
                || double.IsInfinity(massGrams)
                || double.IsNaN(specificHeatCapacity)
                || double.IsInfinity(specificHeatCapacity)
                || currentTemperatureKelvin <= 0.0
                || meltingTemperatureKelvin <= 0.0
                || massGrams <= 0.0
                || specificHeatCapacity <= 0.0)
            {
                return OverloadHeatResult.Invalid();
            }

            var targetTemperatureKelvin = meltingTemperatureKelvin * TargetTemperatureRatio;
            var deltaTemperatureKelvin = Math.Max(0.0, targetTemperatureKelvin - currentTemperatureKelvin);
            var requiredEnergyDtu = massGrams * specificHeatCapacity * deltaTemperatureKelvin;
            return new OverloadHeatResult(true, targetTemperatureKelvin, requiredEnergyDtu, deltaTemperatureKelvin > 0.0);
        }
    }
}
