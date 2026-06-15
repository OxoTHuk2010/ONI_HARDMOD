using System.Reflection;
using Klei.AI;

namespace HardcoreSystems.Modules.DiseaseEffects
{
    internal static class DiseaseEffectsRuntime
    {
        private static bool applied;

        public static bool Enabled { get; private set; }
        public static float Severity { get; private set; }

        public static void Configure(ModContext context)
        {
            Enabled = context.Settings.Diseases.Enabled && context.Settings.Diseases.SeverityMultiplier > 0f;
            Severity = context.Settings.Diseases.SeverityMultiplier;
        }

        public static void ApplyToDatabase(ModContext context)
        {
            if (!Enabled || applied)
            {
                return;
            }

            var db = Db.Get();
            if (db == null || db.Sicknesses == null)
            {
                context.Logger.Warning("disease_database_missing", "Disease database is not available.");
                return;
            }

            AddFoodPoisoningModifiers(db.Sicknesses.FoodSickness, context);
            AddSlimelungModifiers(db.Sicknesses.SlimeSickness, context);
            applied = true;
        }

        private static void AddFoodPoisoningModifiers(Sickness sickness, ModContext context)
        {
            if (sickness == null)
            {
                context.Logger.Warning("food_sickness_missing", "Food Poisoning sickness was not found.");
                return;
            }

            AddComponent(
                sickness,
                new AttributeModifierSickness(
                    new[]
                    {
                        new AttributeModifier(
                            "WorkSpeed",
                            DiseasePenaltyCalculator.ProductivityMultiplier(Severity),
                            "Hardcore Systems Food Poisoning productivity penalty",
                            true,
                            false,
                            true)
                    }),
                context);
        }

        private static void AddSlimelungModifiers(Sickness sickness, ModContext context)
        {
            if (sickness == null)
            {
                context.Logger.Warning("slimelung_sickness_missing", "Slimelung sickness was not found.");
                return;
            }

            AddComponent(
                sickness,
                new AttributeModifierSickness(
                    new[]
                    {
                        new AttributeModifier(
                            "QualityOfLife",
                            DiseasePenaltyCalculator.SlimelungMoralePenalty(Severity),
                            "Hardcore Systems Slimelung morale penalty",
                            false,
                            false,
                            true),
                        new AttributeModifier(
                            "StressDelta",
                            DiseasePenaltyCalculator.StressMultiplier(Severity),
                            "Hardcore Systems Slimelung stress penalty",
                            true,
                            false,
                            true)
                    }),
                context);
        }

        private static void AddComponent(Sickness sickness, Sickness.SicknessComponent component, ModContext context)
        {
            var method = typeof(Sickness).GetMethod("AddSicknessComponent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                context.Logger.Warning("sickness_component_api_missing", "Sickness.AddSicknessComponent was not found.");
                return;
            }

            method.Invoke(sickness, new object[] { component });
        }
    }
}
