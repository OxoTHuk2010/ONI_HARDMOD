using System.IO;
using HarmonyLib;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.WorldGeneration
{
    public sealed class WorldGenerationModule : IGameplayModule
    {
        public string Id
        {
            get { return "WorldGeneration"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.World.Enabled && !IsVanilla(context.Settings.World.AsteroidSize);
        }

        public void Initialize(ModContext context)
        {
            if (!context.Settings.World.Enabled)
            {
                return;
            }

            var worldgenPath = Path.Combine(context.ModPath, "worldgen");
            context.Logger.Info(
                "worldgen_assets_configured",
                "World generation assets are configured for new-world selection.",
                "asteroid_size", context.Settings.World.AsteroidSize,
                "worldgen_path", worldgenPath,
                "worldgen_present", Directory.Exists(worldgenPath).ToString());
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            context.Logger.Info(
                "worldgen_data_only",
                "Asteroid size v0.7 uses data-only cluster/world YAML presets; no runtime worldgen patch is installed.",
                "asteroid_size", context.Settings.World.AsteroidSize);
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current) { }

        public void Shutdown(ModContext context) { }

        private static bool IsVanilla(string value)
        {
            return string.IsNullOrEmpty(value) || string.Equals(value, "Vanilla", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
