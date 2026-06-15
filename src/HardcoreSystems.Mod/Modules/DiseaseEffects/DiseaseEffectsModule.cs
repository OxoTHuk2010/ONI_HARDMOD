using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.DiseaseEffects
{
    public sealed class DiseaseEffectsModule : IGameplayModule
    {
        public string Id
        {
            get { return "DiseaseEffects"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Diseases.Enabled;
        }

        public void Initialize(ModContext context)
        {
            DiseaseEffectsRuntime.Configure(context);
            DiseaseEffectsPatches.Context = context;
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            var target = AccessTools.Method(typeof(Db), "Initialize");
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    target,
                    null,
                    new HarmonyMethod(typeof(DiseaseEffectsPatches), "DbInitializePostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            DiseaseEffectsRuntime.Configure(context);
            DiseaseEffectsPatches.Context = context;
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class DiseaseEffectsPatches
    {
        public static ModContext Context { get; set; }

        public static void DbInitializePostfix()
        {
            if (Context != null)
            {
                DiseaseEffectsRuntime.ApplyToDatabase(Context);
            }
        }
    }
}
