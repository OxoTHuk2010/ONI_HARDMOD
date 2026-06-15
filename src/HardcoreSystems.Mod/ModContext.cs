using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;
using HardcoreSystems.Diagnostics;

namespace HardcoreSystems
{
    public sealed class ModContext
    {
        public ModContext(
            string modId,
            string modVersion,
            string modPath,
            string configPath,
            GameCompatibility compatibility,
            DlcInfo dlc,
            ModSettings settings,
            ModLogger logger)
        {
            ModId = modId;
            ModVersion = modVersion;
            ModPath = modPath;
            ConfigPath = configPath;
            Compatibility = compatibility;
            Dlc = dlc;
            Settings = settings;
            Logger = logger;
        }

        public string ModId { get; private set; }
        public string ModVersion { get; private set; }
        public string ModPath { get; private set; }
        public string ConfigPath { get; private set; }
        public GameCompatibility Compatibility { get; private set; }
        public DlcInfo Dlc { get; private set; }
        public ModSettings Settings { get; private set; }
        public ModLogger Logger { get; private set; }
    }
}
