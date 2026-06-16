using System;
using HardcoreSystems.Diagnostics;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    internal static class ElectricalNetworksRuntime
    {
        private static ModLogger logger;

        public static bool OverloadHeatEnabled { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            OverloadHeatEnabled = context.Settings.Power.OverloadHeatEnabled;
        }

        public static void ApplyDamageHeat(BuildingHP buildingHp)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!OverloadHeatEnabled || buildingHp == null || !IsElectricalNetworkPart(buildingHp))
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var primary = buildingHp.GetComponent<PrimaryElement>();
                if (primary == null || primary.Element == null)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var targetTemperature = ElectricalLossCalculator.CalculateSafeEmergencyTemperature(primary.Element.highTemp);
                if (!ElectricalLossCalculator.ShouldRaiseTemperature(primary.Temperature, targetTemperature))
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                primary.Temperature = targetTemperature;
                DiagnosticsRuntime.Record("ElectricalNetworks", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 0, 1);
                logger.RateLimitedWarning("electrical_damage_heat_failed", "electrical_damage_heat_failed", "Electrical damage heat patch failed and was skipped.");
            }
        }

        private static bool IsElectricalNetworkPart(BuildingHP buildingHp)
        {
            return buildingHp.GetComponent<Wire>() != null
                || buildingHp.GetComponent<WireUtilityNetworkLink>() != null;
        }
    }
}
