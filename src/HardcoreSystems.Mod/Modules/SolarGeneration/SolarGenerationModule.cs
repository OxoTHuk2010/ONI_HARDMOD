using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.SolarGeneration
{
    public sealed class SolarGenerationModule : IGameplayModule
    {
        public string Id
        {
            get { return "SolarGeneration"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Power.SolarPanelGenerationHeatEnabled;
        }

        public void Initialize(ModContext context)
        {
            SolarGenerationRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(SolarPanel), "EnergySim200ms", new[] { typeof(float) }),
                    null,
                    new HarmonyMethod(typeof(SolarGenerationPatches), "EnergySim200msPostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            SolarGenerationRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class SolarGenerationPatches
    {
        public static void EnergySim200msPostfix(SolarPanel __instance, float dt)
        {
            SolarGenerationRuntime.ApplyGenerationHeat(__instance, dt);
        }
    }
}
