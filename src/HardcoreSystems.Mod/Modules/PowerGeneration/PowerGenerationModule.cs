using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.PowerGeneration
{
    public sealed class PowerGenerationModule : IGameplayModule
    {
        public string Id
        {
            get { return "PowerGeneration"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Power.GeneratorRebalanceEnabled;
        }

        public void Initialize(ModContext context)
        {
            PowerGenerationRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(Generator), "WattageRating"),
                    null,
                    new HarmonyMethod(typeof(PowerGenerationPatches), "WattageRatingPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(StructureTemperaturePayload), "OperatingKilowatts"),
                    null,
                    new HarmonyMethod(typeof(PowerGenerationPatches), "OperatingKilowattsPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(Building), "EffectDescriptors", new[] { typeof(BuildingDef) }),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EffectDescriptorsPrefix"),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EffectDescriptorsPostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            PowerGenerationRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class PowerGenerationPatches
    {
        public static void WattageRatingPostfix(Generator __instance, ref float __result)
        {
            PowerGenerationRuntime.ApplyToWattage(__instance, ref __result);
        }

        public static void OperatingKilowattsPostfix(StructureTemperaturePayload __instance, ref float __result)
        {
            PowerGenerationRuntime.ApplyToOperatingKilowatts(__instance, ref __result);
        }

        public static void EffectDescriptorsPrefix(BuildingDef def, out PowerGenerationDescriptorState __state)
        {
            __state = null;
            if (def == null)
            {
                return;
            }

            __state = new PowerGenerationDescriptorState(
                def.GeneratorWattageRating,
                def.SelfHeatKilowattsWhenActive);

            def.GeneratorWattageRating = PowerGenerationRuntime.ApplyToDescriptorWattage(def, def.GeneratorWattageRating);
            def.SelfHeatKilowattsWhenActive = PowerGenerationRuntime.ApplyToDescriptorSelfHeat(def, def.SelfHeatKilowattsWhenActive);
        }

        public static void EffectDescriptorsPostfix(BuildingDef def, PowerGenerationDescriptorState __state)
        {
            if (def != null && __state != null)
            {
                def.GeneratorWattageRating = __state.GeneratorWattageRating;
                def.SelfHeatKilowattsWhenActive = __state.SelfHeatKilowattsWhenActive;
            }
        }
    }

    internal sealed class PowerGenerationDescriptorState
    {
        public PowerGenerationDescriptorState(float generatorWattageRating, float selfHeatKilowattsWhenActive)
        {
            GeneratorWattageRating = generatorWattageRating;
            SelfHeatKilowattsWhenActive = selfHeatKilowattsWhenActive;
        }

        public float GeneratorWattageRating { get; private set; }
        public float SelfHeatKilowattsWhenActive { get; private set; }
    }
}
