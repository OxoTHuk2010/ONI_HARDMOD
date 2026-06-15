using HarmonyLib;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules
{
    public interface IGameplayModule
    {
        string Id { get; }

        bool IsEnabled(ModContext context);

        void Initialize(ModContext context);

        void RegisterPatches(Harmony harmony, ModContext context);

        void OnGameStarted(ModContext context);

        void OnGameLoaded(ModContext context);

        void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current);

        void Shutdown(ModContext context);
    }
}
