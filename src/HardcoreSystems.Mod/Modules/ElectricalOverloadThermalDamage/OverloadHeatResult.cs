namespace HardcoreSystems.Modules.ElectricalOverloadThermalDamage
{
    public struct OverloadHeatResult
    {
        public OverloadHeatResult(bool isValid, double targetTemperatureKelvin, double requiredEnergyDtu, bool shouldApply)
        {
            IsValid = isValid;
            TargetTemperatureKelvin = targetTemperatureKelvin;
            RequiredEnergyDtu = requiredEnergyDtu;
            ShouldApply = shouldApply;
        }

        public bool IsValid { get; private set; }
        public double TargetTemperatureKelvin { get; private set; }
        public double RequiredEnergyDtu { get; private set; }
        public bool ShouldApply { get; private set; }

        public static OverloadHeatResult Invalid()
        {
            return new OverloadHeatResult(false, 0.0, 0.0, false);
        }
    }
}
