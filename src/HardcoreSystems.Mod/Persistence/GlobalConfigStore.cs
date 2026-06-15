using System;
using System.IO;
using HardcoreSystems.Configuration;
using HardcoreSystems.Diagnostics;
using Newtonsoft.Json;

namespace HardcoreSystems.Persistence
{
    public sealed class GlobalConfigStore
    {
        private readonly string path;
        private readonly ModLogger logger;

        public GlobalConfigStore(string path, ModLogger logger)
        {
            this.path = path;
            this.logger = logger;
        }

        public ModSettings LoadOrDefault()
        {
            if (!File.Exists(path))
            {
                var settings = PresetFactory.Create(DifficultyPreset.Off);
                Save(settings);
                return settings;
            }

            try
            {
                return JsonConvert.DeserializeObject<ModSettings>(File.ReadAllText(path)) ?? PresetFactory.Create(DifficultyPreset.Off);
            }
            catch (Exception ex)
            {
                logger.Warning(
                    "config_load_failed",
                    "Config load failed; using Off preset.",
                    "exception", ex.GetType().Name,
                    "message", ex.Message);
                return PresetFactory.Create(DifficultyPreset.Off);
            }
        }

        public void Save(ModSettings settings)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}
