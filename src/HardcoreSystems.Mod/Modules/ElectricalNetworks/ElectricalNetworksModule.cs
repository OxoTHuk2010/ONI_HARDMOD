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
                || context.Settings.Power.TransformerEfficiencyEnabled
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
                    AccessTools.Method(typeof(PowerTransformer), "ApplyDeltaJoules", new[] { typeof(float), typeof(bool) }),
                    new HarmonyMethod(typeof(ElectricalNetworksPatches), "PowerTransformerApplyDeltaJoulesPrefix"),
                    null,
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(Battery), "GetDescriptors", new[] { typeof(UnityEngine.GameObject) }),
                    null,
                    new HarmonyMethod(typeof(ElectricalNetworksPatches), "BatteryGetDescriptorsPostfix"),
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

        public static void PowerTransformerApplyDeltaJoulesPrefix(float joules_delta, Battery ___battery)
        {
            ElectricalNetworksRuntime.ApplyTransformerEfficiency(joules_delta, ___battery);
        }

        public static void BatteryGetDescriptorsPostfix(Battery __instance, List<Descriptor> __result)
        {
            ElectricalNetworksRuntime.AddTransformerDescriptor(__instance, __result);
        }
    }
}
