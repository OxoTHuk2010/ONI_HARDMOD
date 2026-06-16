using System;
using HardcoreSystems.Diagnostics;

namespace HardcoreSystems.Modules.PowerGeneration
{
    internal static class PowerGenerationRuntime
    {
        private static ModLogger logger;

        public static bool Enabled { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            Enabled = context.Settings.Power.GeneratorRebalanceEnabled;
        }

        public static void ApplyToWattage(Generator generator, ref float watts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                PowerGenerationProfile profile;
                if (!Enabled || !TryGetProfile(generator, out profile))
                {
                    DiagnosticsRuntime.Record("PowerGeneration", start, 0, 1, 0);
                    return;
                }

                watts = profile.Wattage;
                DiagnosticsRuntime.Record("PowerGeneration", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("PowerGeneration", start, 0, 0, 1);
                logger.RateLimitedWarning("power_generation_wattage_failed", "power_generation_wattage_failed", "Power generation wattage patch failed and was skipped.");
            }
        }

        public static void ApplyToOperatingKilowatts(StructureTemperaturePayload payload, ref float kilowatts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                PowerGenerationProfile profile;
                if (!Enabled || payload.building == null || !PowerGenerationProfileRegistry.TryGet(payload.building.Def.PrefabID, out profile))
                {
                    DiagnosticsRuntime.Record("PowerGeneration", start, 0, 1, 0);
                    return;
                }

                kilowatts = profile.BodyHeatKilowatts;
                DiagnosticsRuntime.Record("PowerGeneration", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("PowerGeneration", start, 0, 0, 1);
                logger.RateLimitedWarning("power_generation_heat_failed", "power_generation_heat_failed", "Power generation heat patch failed and was skipped.");
            }
        }

        public static float ApplyToDescriptorWattage(BuildingDef def, float watts)
        {
            PowerGenerationProfile profile;
            if (!Enabled || def == null || !PowerGenerationProfileRegistry.TryGet(def.PrefabID, out profile))
            {
                return watts;
            }

            return profile.Wattage;
        }

        public static float ApplyToDescriptorSelfHeat(BuildingDef def, float selfHeatKilowatts)
        {
            PowerGenerationProfile profile;
            if (!Enabled || def == null || !PowerGenerationProfileRegistry.TryGet(def.PrefabID, out profile))
            {
                return selfHeatKilowatts;
            }

            return profile.BodyHeatKilowatts;
        }

        public static bool ShouldOwnGeneratorHeat(BuildingDef def)
        {
            return Enabled && PowerGenerationProfileRegistry.IsProfiledGenerator(def);
        }

        private static bool TryGetProfile(Generator generator, out PowerGenerationProfile profile)
        {
            profile = null;
            if (generator == null)
            {
                return false;
            }

            var building = generator.GetComponent<Building>();
            return building != null && building.Def != null && PowerGenerationProfileRegistry.TryGet(building.Def.PrefabID, out profile);
        }
    }
}
