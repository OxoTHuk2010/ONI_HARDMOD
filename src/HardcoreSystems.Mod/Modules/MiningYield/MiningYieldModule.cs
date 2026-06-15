using System;
using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.MiningYield
{
    public sealed class MiningYieldModule : IGameplayModule
    {
        public string Id
        {
            get { return "MiningYield"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Mining.Enabled;
        }

        public void Initialize(ModContext context)
        {
            MiningYieldRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            var onDigComplete = AccessTools.Method(
                typeof(WorldDamage),
                "OnDigComplete",
                new[] { typeof(int), typeof(float), typeof(float), typeof(ushort), typeof(byte), typeof(int) });
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    onDigComplete,
                    new HarmonyMethod(typeof(MiningYieldPatches), "OnDigCompletePrefix"),
                    null,
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            MiningYieldRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class MiningYieldPatches
    {
        public static void OnDigCompletePrefix(int __0, ref float __1, ushort __3)
        {
            MiningYieldRuntime.ApplyToCompletedDig(__0, ref __1, __3);
        }
    }
}
