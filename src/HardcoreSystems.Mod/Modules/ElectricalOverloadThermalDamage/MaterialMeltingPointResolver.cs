namespace HardcoreSystems.Modules.ElectricalOverloadThermalDamage
{
    internal static class MaterialMeltingPointResolver
    {
        public static bool TryResolve(PrimaryElement primary, out float meltingTemperatureKelvin, out float specificHeatCapacity)
        {
            meltingTemperatureKelvin = 0f;
            specificHeatCapacity = 0f;

            if (primary == null || primary.Element == null)
            {
                return false;
            }

            var element = primary.Element;
            if (float.IsNaN(element.highTemp)
                || float.IsInfinity(element.highTemp)
                || element.highTemp <= 0f
                || float.IsNaN(element.specificHeatCapacity)
                || float.IsInfinity(element.specificHeatCapacity)
                || element.specificHeatCapacity <= 0f)
            {
                return false;
            }

            meltingTemperatureKelvin = element.highTemp;
            specificHeatCapacity = element.specificHeatCapacity;
            return true;
        }
    }
}
