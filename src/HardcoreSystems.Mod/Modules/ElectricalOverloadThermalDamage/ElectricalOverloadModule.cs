using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.ElectricalOverloadThermalDamage
{
    public sealed class ElectricalOverloadModule : IGameplayModule
    {
        public string Id
        {
            get { return "ElectricalOverloadThermalDamage"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Power.OverloadHeatEnabled;
        }

        public void Initialize(ModContext context)
        {
            ElectricalOverloadRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(BuildingHP), "DoDamage", new[] { typeof(int) }),
                    null,
                    new HarmonyMethod(typeof(ElectricalOverloadPatches), "BuildingHpDoDamagePostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            ElectricalOverloadRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class ElectricalOverloadPatches
    {
        public static void BuildingHpDoDamagePostfix(BuildingHP __instance, int damage)
        {
            ElectricalOverloadRuntime.ApplyDamageHeat(__instance, damage);
        }
    }
}
