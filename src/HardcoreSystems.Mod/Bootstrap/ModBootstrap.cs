using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using HardcoreSystems.Configuration;
using HardcoreSystems.Diagnostics;
using HardcoreSystems.Modules.DiseaseEffects;
using HardcoreSystems.Modules.DuplicantBalance;
using HardcoreSystems.Modules.ElectricalOverloadThermalDamage;
using HardcoreSystems.Modules.IndustrialHeat;
using HardcoreSystems.Modules.MiningYield;
using HardcoreSystems.Modules.SolarGeneration;
using HardcoreSystems.Persistence;

namespace HardcoreSystems.Bootstrap
{
    public static class ModBootstrap
    {
        public const string ModId = "oxygen.hardcore.systems";
        public const string DisplayName = "Hardcore Systems";

        public static ModContext Current { get; private set; }

        public static void Initialize(KMod.UserMod2 userMod, Harmony harmony)
        {
            var modPath = userMod.path;
            var configPath = Path.Combine(modPath, "config", "hardcore_systems.json");
            var logger = new ModLogger(DisplayName, LogLevel.Info);
            var store = new GlobalConfigStore(configPath, logger);
            var settings = store.LoadOrDefault();
            var validation = SettingsValidator.Validate(settings);

            if (!validation.IsValid)
            {
                logger.Warning("config_validation_failed", "Invalid config was replaced with the Off preset.", validation.ToLogFields());
                settings = PresetFactory.Create(DifficultyPreset.Off);
                store.Save(settings);
            }

            var compatibility = GameCompatibility.Capture();
            var dlc = DlcDetector.Detect();
            Current = new ModContext(
                ModId,
                Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                modPath,
                configPath,
                compatibility,
                dlc,
                settings,
                logger);

            LocalizationRegistrar.Register(Current);
            DiagnosticsRuntime.Configure(Current);
            var registry = new ModuleRegistry(logger);
            registry.Register(new MiningYieldModule());
            registry.Register(new DuplicantBalanceModule());
            registry.Register(new DiseaseEffectsModule());
            registry.Register(new SolarGenerationModule());
            registry.Register(new IndustrialHeatModule());
            registry.Register(new ElectricalOverloadModule());
            registry.Initialize(Current);
            registry.RegisterPatches(harmony, Current);

            logger.Info(
                "mod_loaded",
                "Hardcore Systems loaded.",
                "version", Current.ModVersion,
                "unity", compatibility.UnityVersion,
                "dlc", dlc.ToSummary());
        }
    }
}
