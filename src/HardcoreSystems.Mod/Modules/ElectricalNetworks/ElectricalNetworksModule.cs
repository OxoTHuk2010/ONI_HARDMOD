using System.Collections.Generic;
using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    public sealed class ElectricalNetworksModule : IGameplayModule
    {
        public string Id
        {
            get { return "ElectricalNetworks"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Power.ElectricalLossesEnabled
                || context.Settings.Power.OverloadHeatEnabled;
        }

        public void Initialize(ModContext context)
        {
            ElectricalNetworksRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(Wire), "OnSpawn"),
                    null,
                    new HarmonyMethod(typeof(ElectricalNetworksPatches), "WireOnSpawnPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(Wire), "OnCleanUp"),
                    new HarmonyMethod(typeof(ElectricalNetworksPatches), "WireOnCleanUpPrefix"),
                    null,
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(CircuitManager), "Sim200msFirst", new[] { typeof(float) }),
                    null,
                    new HarmonyMethod(typeof(ElectricalNetworksPatches), "CircuitSim200msFirstPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(EnergyConsumer), "EnergySim200ms", new[] { typeof(float) }),
                    null,
                    new HarmonyMethod(typeof(ElectricalNetworksPatches), "EnergyConsumerSim200msPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(BuildingHP), "DoDamage", new[] { typeof(int) }),
                    null,
                    new HarmonyMethod(typeof(ElectricalNetworksPatches), "BuildingHpDoDamagePostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            ElectricalNetworksRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class ElectricalNetworksPatches
    {
        public static void WireOnSpawnPostfix(Wire __instance)
        {
            ElectricalNetworksRuntime.RegisterWire(__instance);
        }

        public static void WireOnCleanUpPrefix(Wire __instance)
        {
            ElectricalNetworksRuntime.UnregisterWire(__instance);
        }

        public static void CircuitSim200msFirstPostfix(CircuitManager __instance, float dt)
        {
            ElectricalNetworksRuntime.SimulateCircuitTick(__instance, dt);
        }

        public static void EnergyConsumerSim200msPostfix(EnergyConsumer __instance, float dt)
        {
            ElectricalNetworksRuntime.ApplyConsumerBrownout(__instance, dt);
        }

        public static void BuildingHpDoDamagePostfix(BuildingHP __instance, int damage)
        {
            ElectricalNetworksRuntime.ApplyDamageImpulse(__instance, damage);
        }
    }
}
