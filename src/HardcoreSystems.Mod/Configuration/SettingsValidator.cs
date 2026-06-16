using System;

namespace HardcoreSystems.Configuration
{
    public static class SettingsValidator
    {
        public static ValidationResult Validate(ModSettings settings)
        {
            var result = new ValidationResult();
            if (settings == null)
            {
                result.Add("settings is null");
                return result;
            }

            Required(settings.Global, "Global", result);
            Required(settings.World, "World", result);
            Required(settings.Mining, "Mining", result);
            Required(settings.RadiativeHeat, "RadiativeHeat", result);
            Required(settings.Power, "Power", result);
            Required(settings.IndustrialHeat, "IndustrialHeat", result);
            Required(settings.FluidPressure, "FluidPressure", result);
            Required(settings.Diseases, "Diseases", result);
            Required(settings.Duplicants, "Duplicants", result);
            Required(settings.Diagnostics, "Diagnostics", result);

            if (!result.IsValid)
            {
                return result;
            }

            Range(settings.Mining.YieldMultiplier, 0.01f, 1.00f, "Mining.YieldMultiplier", result);
            Range(settings.Duplicants.ExperienceMultiplier, 0.05f, 10.00f, "Duplicants.ExperienceMultiplier", result);
            Range(settings.Duplicants.CaloriesMultiplier, 0.10f, 10.00f, "Duplicants.CaloriesMultiplier", result);
            Range(settings.Diseases.SeverityMultiplier, 0.00f, 0.95f, "Diseases.SeverityMultiplier", result);
            Range(settings.IndustrialHeat.HeatMultiplier, 0.00f, 10.00f, "IndustrialHeat.HeatMultiplier", result);
            Range(settings.World.TemperatureMultiplier, 0.25f, 4.00f, "World.TemperatureMultiplier", result);
            Range(settings.World.TemperatureOffsetCelsius, -200f, 500f, "World.TemperatureOffsetCelsius", result);
            Range(settings.FluidPressure.LiquidFillThreshold, 0.01f, 0.50f, "FluidPressure.LiquidFillThreshold", result);
            Range(settings.FluidPressure.SimulationIntervalSeconds, 0.20f, 600f, "FluidPressure.SimulationIntervalSeconds", result);
            Range(settings.RadiativeHeat.SimulationIntervalSeconds, 0.20f, 600f, "RadiativeHeat.SimulationIntervalSeconds", result);
            Range(settings.Diagnostics.ReportIntervalSeconds, 1f, 600f, "Diagnostics.ReportIntervalSeconds", result);
            return result;
        }

        private static void Required(object value, string name, ValidationResult result)
        {
            if (value == null)
            {
                result.Add(name + " is required");
            }
        }

        private static void Range(float value, float min, float max, string name, ValidationResult result)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < min || value > max)
            {
                result.Add(name + " must be between " + min.ToString("0.###") + " and " + max.ToString("0.###"));
            }
        }
    }
}
