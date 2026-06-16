using System;
using HardcoreSystems.Diagnostics;
using HardcoreSystems.Modules.Thermal;

namespace HardcoreSystems.Modules.SolarGeneration
{
    internal static class SolarGenerationRuntime
    {
        private static readonly SolarPanelProfile Profile = SolarPanelProfile.Vanilla();
        private static ModLogger logger;

        public static bool Enabled { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            Enabled = context.Settings.Power.SolarPanelGenerationHeatEnabled;
        }

        public static void ApplyGenerationHeat(SolarPanel panel, float deltaTimeSeconds)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!Enabled || panel == null || deltaTimeSeconds <= 0f)
                {
                    DiagnosticsRuntime.Record("SolarGeneration", start, 0, 1, 0);
                    return;
                }

                var heatDtuPerSecond = SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(
                    panel.CurrentWattage,
                    Profile.MaximumPowerWatts,
                    Profile.MaximumHeatDtuPerSecond);
                var energyDtu = SolarHeatCalculator.CalculateEnergyDtu(heatDtuPerSecond, deltaTimeSeconds);
                if (energyDtu <= 0.0)
                {
                    DiagnosticsRuntime.Record("SolarGeneration", start, 0, 1, 0);
                    return;
                }

                if (!BuildingThermalAdapter.TryAddEnergy(panel.gameObject, energyDtu, 1f, 10000f))
                {
                    DiagnosticsRuntime.Record("SolarGeneration", start, 0, 1, 0);
                    logger.RateLimitedWarning("solar_generation_heat_no_sim_handle", "solar_generation_heat_no_sim_handle", "Solar panel heat was skipped because the building thermal sim handle is not available.");
                    return;
                }

                DiagnosticsRuntime.Record("SolarGeneration", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("SolarGeneration", start, 0, 0, 1);
                logger.RateLimitedWarning("solar_generation_heat_failed", "solar_generation_heat_failed", "Solar panel heat patch failed and was skipped.");
            }
        }

        public static float ApplyToDescriptorSelfHeat(BuildingDef def, float selfHeatKilowatts)
        {
            if (!Enabled || def == null || def.PrefabID != "SolarPanel")
            {
                return selfHeatKilowatts;
            }

            return (float)(Profile.MaximumHeatDtuPerSecond / 1000.0);
        }

        public static void ApplyToTotalEnergyProduced(StructureTemperaturePayload payload, ref float kilowatts)
        {
            if (!Enabled || payload.building == null || payload.building.Def == null || payload.building.Def.PrefabID != "SolarPanel")
            {
                return;
            }

            var panel = payload.building.GetComponent<SolarPanel>();
            if (panel == null)
            {
                return;
            }

            kilowatts = (float)(SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(
                panel.CurrentWattage,
                Profile.MaximumPowerWatts,
                Profile.MaximumHeatDtuPerSecond) / 1000.0);
        }
    }
}
