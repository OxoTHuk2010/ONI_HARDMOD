using System;
using HardcoreSystems.Diagnostics;
using HardcoreSystems.Modules.Thermal;

namespace HardcoreSystems.Modules.ElectricalOverloadThermalDamage
{
    internal static class ElectricalOverloadRuntime
    {
        private static readonly OverloadEventDeduplicator Deduplicator = new OverloadEventDeduplicator();
        private static ModLogger logger;

        public static bool Enabled { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            Enabled = context.Settings.Power.OverloadHeatEnabled;
        }

        public static void ApplyDamageHeat(BuildingHP buildingHp, int damage)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!Enabled || damage <= 0 || buildingHp == null || !IsOverloadDamage(buildingHp) || !IsElectricalConductor(buildingHp))
                {
                    DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 0, 1, 0);
                    return;
                }

                if (!Deduplicator.TryEnter(buildingHp.gameObject))
                {
                    DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 0, 1, 0);
                    return;
                }

                var primary = buildingHp.GetComponent<PrimaryElement>();
                float meltingTemperature;
                float specificHeatCapacity;
                if (primary == null || !MaterialMeltingPointResolver.TryResolve(primary, out meltingTemperature, out specificHeatCapacity))
                {
                    DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 0, 1, 0);
                    logger.RateLimitedWarning("overload_heat_invalid_material", "overload_heat_invalid_material", "Electrical overload heat skipped because material thermal data is missing.");
                    return;
                }

                var result = OverloadHeatCalculator.CalculateOverloadHeatFromKilograms(
                    primary.Temperature,
                    meltingTemperature,
                    primary.Mass,
                    specificHeatCapacity);
                if (!result.IsValid)
                {
                    DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 0, 1, 0);
                    logger.RateLimitedWarning("overload_heat_invalid_calculation", "overload_heat_invalid_calculation", "Electrical overload heat calculation returned invalid data.");
                    return;
                }

                if (!result.ShouldApply)
                {
                    DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 0, 1, 0);
                    return;
                }

                if (!BuildingThermalAdapter.TryAddEnergy(buildingHp.gameObject, result.RequiredEnergyDtu, primary.Temperature, (float)result.TargetTemperatureKelvin))
                {
                    DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 0, 1, 0);
                    logger.RateLimitedWarning("overload_heat_no_sim_handle", "overload_heat_no_sim_handle", "Electrical overload heat skipped because the building thermal sim handle is not available.");
                    return;
                }

                DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("ElectricalOverloadThermalDamage", start, 0, 0, 1);
                logger.RateLimitedWarning("overload_heat_failed", "overload_heat_failed", "Electrical overload heat patch failed and was skipped.");
            }
        }

        private static bool IsElectricalConductor(BuildingHP buildingHp)
        {
            return buildingHp.GetComponent<Wire>() != null
                || buildingHp.GetComponent<WireUtilityNetworkLink>() != null
                || (buildingHp.GetComponent<IWattageRating>() != null && buildingHp.GetComponent<IBridgedNetworkItem>() != null);
        }

        private static bool IsOverloadDamage(BuildingHP buildingHp)
        {
            var source = buildingHp.GetDamageSourceInfo();
            if (Contains(source.statusItemID, "overload") || Contains(source.statusItemID, "Overloaded"))
            {
                return true;
            }

            return Contains(source.source, "overload")
                || Contains(source.source, "перегруз")
                || Contains(source.popString, "overload")
                || Contains(source.popString, "перегруз");
        }

        private static bool Contains(string value, string pattern)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
