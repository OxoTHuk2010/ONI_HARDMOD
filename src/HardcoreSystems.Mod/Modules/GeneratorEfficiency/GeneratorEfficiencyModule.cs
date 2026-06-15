using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.GeneratorEfficiency
{
    public sealed class GeneratorEfficiencyModule : IGameplayModule
    {
        public string Id
        {
            get { return "GeneratorEfficiency"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Power.GeneratorEfficiencyEnabled;
        }

        public void Initialize(ModContext context)
        {
            GeneratorEfficiencyRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(Generator), "WattageRating"),
                    null,
                    new HarmonyMethod(typeof(GeneratorEfficiencyPatches), "WattageRatingPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(Building), "EffectDescriptors", new[] { typeof(BuildingDef) }),
                    new HarmonyMethod(typeof(GeneratorEfficiencyPatches), "EffectDescriptorsPrefix"),
                    new HarmonyMethod(typeof(GeneratorEfficiencyPatches), "EffectDescriptorsPostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            GeneratorEfficiencyRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class GeneratorEfficiencyPatches
    {
        public static void WattageRatingPostfix(ref float __result)
        {
            GeneratorEfficiencyRuntime.ApplyToWattage(ref __result);
        }

        public static void EffectDescriptorsPrefix(BuildingDef def, out float __state)
        {
            __state = 0f;
            if (def == null || def.GeneratorWattageRating <= 0f)
            {
                return;
            }

            __state = def.GeneratorWattageRating;
            def.GeneratorWattageRating = GeneratorEfficiencyRuntime.ApplyToDescriptorWattage(def.GeneratorWattageRating);
        }

        public static void EffectDescriptorsPostfix(BuildingDef def, float __state)
        {
            if (def != null && __state > 0f)
            {
                def.GeneratorWattageRating = __state;
            }
        }
    }
}
