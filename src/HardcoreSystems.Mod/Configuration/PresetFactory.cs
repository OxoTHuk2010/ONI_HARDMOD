namespace HardcoreSystems.Configuration
{
    public static class PresetFactory
    {
        public static ModSettings Create(DifficultyPreset preset)
        {
            var settings = new ModSettings();
            settings.Preset = preset;
            settings.Global.LastSelectedPreset = preset;

            if (preset == DifficultyPreset.Off || preset == DifficultyPreset.Custom)
            {
                return settings;
            }

            ApplyRuntimeModules(settings, true);

            if (preset == DifficultyPreset.VanillaPlus)
            {
                ApplyValues(settings, 0.50f, 0.80f, 1.10f, 0.10f, 0.95f, 0.25f);
            }
            else if (preset == DifficultyPreset.Hard)
            {
                ApplyValues(settings, 0.25f, 0.60f, 1.25f, 0.30f, 0.90f, 0.50f);
            }
            else if (preset == DifficultyPreset.Extreme)
            {
                ApplyValues(settings, 0.10f, 0.35f, 1.50f, 0.50f, 0.85f, 0.75f);
            }
            else if (preset == DifficultyPreset.Insane)
            {
                ApplyValues(settings, 0.01f, 0.15f, 2.00f, 0.80f, 0.75f, 1.00f);
            }

            return settings;
        }

        private static void ApplyRuntimeModules(ModSettings settings, bool enabled)
        {
            settings.Mining.Enabled = enabled;
            settings.Power.GeneratorRebalanceEnabled = enabled;
            settings.Power.FuelThermalAccountingEnabled = false;
            settings.Power.SolarPanelGenerationHeatEnabled = enabled;
            settings.Power.ElectricalDiagnosticsEnabled = false;
            settings.Power.GeneratorEfficiencyEnabled = false;
            settings.Power.ElectricalLossesEnabled = false;
            settings.Power.TransformerEfficiencyEnabled = false;
            settings.Power.OverloadHeatEnabled = enabled;
            settings.IndustrialHeat.Enabled = enabled;
            settings.Diseases.Enabled = enabled;
            settings.Duplicants.Enabled = enabled;
        }

        private static void ApplyValues(
            ModSettings settings,
            float miningYield,
            float experience,
            float calories,
            float diseaseSeverity,
            float transformerEfficiency,
            float wireResistance)
        {
            settings.Mining.YieldMultiplier = miningYield;
            settings.Duplicants.ExperienceMultiplier = experience;
            settings.Duplicants.CaloriesMultiplier = calories;
            settings.Diseases.SeverityMultiplier = diseaseSeverity;
            settings.Power.GeneratorEfficiency = 1f;
            settings.Power.TransformerEfficiency = 1f;
            settings.Power.WireResistanceMultiplier = 0f;
            settings.IndustrialHeat.HeatMultiplier = 1f + wireResistance;
        }
    }
}
