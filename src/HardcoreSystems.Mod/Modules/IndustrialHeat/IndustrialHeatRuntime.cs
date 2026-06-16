using System;
using HardcoreSystems.Diagnostics;
using HardcoreSystems.Modules.PowerGeneration;

namespace HardcoreSystems.Modules.IndustrialHeat
{
    internal static class IndustrialHeatRuntime
    {
        private static ModLogger logger;

        public static bool Enabled { get; private set; }
        public static float HeatMultiplier { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            var settings = context.Settings.IndustrialHeat;
            Enabled = settings.Enabled && settings.HeatMultiplier > 0.0001f;
            HeatMultiplier = settings.HeatMultiplier;
        }

        public static void ApplyToOperatingKilowatts(StructureTemperaturePayload payload, ref float kilowatts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!Enabled)
                {
                    DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 1, 0);
                    return;
                }

                if (PowerGenerationRuntime.ShouldOwnGeneratorHeat(payload.building == null ? null : payload.building.Def))
                {
                    DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 1, 0);
                    return;
                }

                if (kilowatts <= 0f)
                {
                    if (payload.operational == null || !payload.operational.IsActive)
                    {
                        DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 1, 0);
                        return;
                    }

                    kilowatts = GetPumpFallbackKilowatts(payload.building == null ? null : payload.building.Def);
                }

                if (kilowatts <= 0f)
                {
                    DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 1, 0);
                    return;
                }

                kilowatts = IndustrialHeatCalculator.ScaleOperatingKilowatts(kilowatts, HeatMultiplier);
                DiagnosticsRuntime.Record("IndustrialHeat", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 0, 1);
                logger.RateLimitedWarning("industrial_heat_apply_failed", "industrial_heat_apply_failed", "Industrial heat patch failed and was skipped.");
            }
        }

        public static void ApplyToExhaustKilowatts(StructureTemperaturePayload payload, ref float kilowatts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (ShouldSkipIndustrialHeat(payload.building == null ? null : payload.building.Def))
                {
                    DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 1, 0);
                    return;
                }

                if (!Enabled || kilowatts <= 0f)
                {
                    DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 1, 0);
                    return;
                }

                kilowatts = IndustrialHeatCalculator.ScaleOperatingKilowatts(kilowatts, HeatMultiplier);
                DiagnosticsRuntime.Record("IndustrialHeat", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("IndustrialHeat", start, 0, 0, 1);
                logger.RateLimitedWarning("industrial_heat_exhaust_apply_failed", "industrial_heat_exhaust_apply_failed", "Industrial exhaust heat patch failed and was skipped.");
            }
        }

        public static float ApplyToDescriptorSelfHeat(BuildingDef def, float selfHeatKilowatts, float exhaustKilowatts)
        {
            if (!Enabled)
            {
                return selfHeatKilowatts;
            }

            if (ShouldSkipIndustrialHeat(def))
            {
                return selfHeatKilowatts;
            }

            if (selfHeatKilowatts > 0f)
            {
                return IndustrialHeatCalculator.ScaleOperatingKilowatts(selfHeatKilowatts, HeatMultiplier);
            }

            if (exhaustKilowatts > 0f)
            {
                return selfHeatKilowatts;
            }

            return IndustrialHeatCalculator.ScaleOperatingKilowatts(GetPumpFallbackKilowatts(def), HeatMultiplier);
        }

        public static float ApplyToDescriptorExhaustHeat(BuildingDef def, float heatKilowatts)
        {
            if (!Enabled || ShouldSkipIndustrialHeat(def))
            {
                return heatKilowatts;
            }

            return IndustrialHeatCalculator.ScaleOperatingKilowatts(heatKilowatts, HeatMultiplier);
        }

        private static bool ShouldSkipIndustrialHeat(BuildingDef def)
        {
            if (def == null)
            {
                return false;
            }

            return PowerGenerationRuntime.ShouldOwnGeneratorHeat(def)
                || string.Equals(def.PrefabID, "SolarPanel", StringComparison.OrdinalIgnoreCase);
        }

        private static float GetPumpFallbackKilowatts(BuildingDef def)
        {
            if (def == null || !def.RequiresPowerInput || def.EnergyConsumptionWhenActive <= 0f)
            {
                return 0f;
            }

            if ((def.SelfHeatKilowattsWhenActive + def.ExhaustKilowattsWhenActive) > 0f)
            {
                return 0f;
            }

            var id = def.PrefabID ?? string.Empty;
            if (id.IndexOf("Pump", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return 0f;
            }

            return IndustrialHeatCalculator.CalculatePumpFallbackKilowatts(def.EnergyConsumptionWhenActive);
        }
    }
}
