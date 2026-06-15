using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.IndustrialHeat
{
    public sealed class IndustrialHeatModule : IGameplayModule
    {
        public string Id
        {
            get { return "IndustrialHeat"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.IndustrialHeat.Enabled;
        }

        public void Initialize(ModContext context)
        {
            IndustrialHeatRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(StructureTemperaturePayload), "OperatingKilowatts"),
                    null,
                    new HarmonyMethod(typeof(IndustrialHeatPatches), "OperatingKilowattsPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(StructureTemperaturePayload), "ExhaustKilowatts"),
                    null,
                    new HarmonyMethod(typeof(IndustrialHeatPatches), "ExhaustKilowattsPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(Building), "EffectDescriptors", new[] { typeof(BuildingDef) }),
                    new HarmonyMethod(typeof(IndustrialHeatPatches), "EffectDescriptorsPrefix"),
                    new HarmonyMethod(typeof(IndustrialHeatPatches), "EffectDescriptorsPostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            IndustrialHeatRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class IndustrialHeatPatches
    {
        public static void OperatingKilowattsPostfix(StructureTemperaturePayload __instance, ref float __result)
        {
            IndustrialHeatRuntime.ApplyToOperatingKilowatts(__instance, ref __result);
        }

        public static void ExhaustKilowattsPostfix(StructureTemperaturePayload __instance, ref float __result)
        {
            IndustrialHeatRuntime.ApplyToExhaustKilowatts(ref __result);
        }

        public static void EffectDescriptorsPrefix(BuildingDef def, out HeatDescriptorState __state)
        {
            __state = null;
            if (def == null)
            {
                return;
            }

            __state = new HeatDescriptorState(def.SelfHeatKilowattsWhenActive, def.ExhaustKilowattsWhenActive);
            def.SelfHeatKilowattsWhenActive = IndustrialHeatRuntime.ApplyToDescriptorSelfHeat(
                def,
                def.SelfHeatKilowattsWhenActive,
                def.ExhaustKilowattsWhenActive);
            def.ExhaustKilowattsWhenActive = IndustrialHeatRuntime.ApplyToDescriptorExhaustHeat(def.ExhaustKilowattsWhenActive);
        }

        public static void EffectDescriptorsPostfix(BuildingDef def, HeatDescriptorState __state)
        {
            if (def != null && __state != null)
            {
                def.SelfHeatKilowattsWhenActive = __state.SelfHeatKilowatts;
                def.ExhaustKilowattsWhenActive = __state.ExhaustKilowatts;
            }
        }

        public sealed class HeatDescriptorState
        {
            public HeatDescriptorState(float selfHeatKilowatts, float exhaustKilowatts)
            {
                SelfHeatKilowatts = selfHeatKilowatts;
                ExhaustKilowatts = exhaustKilowatts;
            }

            public float SelfHeatKilowatts { get; private set; }
            public float ExhaustKilowatts { get; private set; }
        }
    }
}
