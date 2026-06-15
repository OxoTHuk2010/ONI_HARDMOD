using System;
using HardcoreSystems.Diagnostics;

namespace HardcoreSystems.Modules.GeneratorEfficiency
{
    internal static class GeneratorEfficiencyRuntime
    {
        private static ModLogger logger;

        public static bool Enabled { get; private set; }
        public static float Efficiency { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            var settings = context.Settings.Power;
            Enabled = settings.GeneratorEfficiencyEnabled && settings.GeneratorEfficiency < 0.9999f;
            Efficiency = settings.GeneratorEfficiency;
        }

        public static void ApplyToWattage(ref float watts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!Enabled)
                {
                    DiagnosticsRuntime.Record("GeneratorEfficiency", start, 0, 1, 0);
                    return;
                }

                watts = GeneratorEfficiencyCalculator.Apply(watts, Efficiency);
                DiagnosticsRuntime.Record("GeneratorEfficiency", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("GeneratorEfficiency", start, 0, 0, 1);
                logger.RateLimitedWarning("generator_efficiency_apply_failed", "generator_efficiency_apply_failed", "Generator efficiency patch failed and was skipped.");
            }
        }

        public static float ApplyToDescriptorWattage(float watts)
        {
            if (!Enabled)
            {
                return watts;
            }

            return GeneratorEfficiencyCalculator.Apply(watts, Efficiency);
        }
    }
}
