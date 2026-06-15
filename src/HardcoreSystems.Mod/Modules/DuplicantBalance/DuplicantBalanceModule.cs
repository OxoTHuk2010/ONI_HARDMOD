using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.DuplicantBalance
{
    public sealed class DuplicantBalanceModule : IGameplayModule
    {
        public string Id
        {
            get { return "DuplicantBalance"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Duplicants.Enabled;
        }

        public void Initialize(ModContext context)
        {
            DuplicantBalanceRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(MinionModifiers), "OnSpawn"),
                    null,
                    new HarmonyMethod(typeof(DuplicantBalancePatches), "MinionModifiersOnSpawnPostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            DuplicantBalanceRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class DuplicantBalancePatches
    {
        public static void MinionModifiersOnSpawnPostfix(MinionModifiers __instance)
        {
            DuplicantBalanceRuntime.ApplyRuntimeCalorieModifier(__instance.gameObject);
        }
    }
}
