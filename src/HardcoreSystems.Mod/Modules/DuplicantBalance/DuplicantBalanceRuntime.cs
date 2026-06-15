using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HardcoreSystems.Diagnostics;
using UnityEngine;

namespace HardcoreSystems.Modules.DuplicantBalance
{
    internal static class DuplicantBalanceRuntime
    {
        private static readonly string[] SkillExperienceFields =
        {
            "FULL_EXPERIENCE",
            "ALL_DAY_EXPERIENCE",
            "MOST_DAY_EXPERIENCE",
            "PART_DAY_EXPERIENCE",
            "BARELY_EVER_EXPERIENCE"
        };

        private static readonly Dictionary<string, float> OriginalSkillExperience = new Dictionary<string, float>();
        private static readonly HashSet<int> CalorieModifierAppliedTo = new HashSet<int>();
        private static ModLogger logger;
        private static bool experienceApplied;

        public static bool CaloriesEnabled { get; private set; }
        public static float CaloriesMultiplier { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            var settings = context.Settings.Duplicants;
            CaloriesEnabled = settings.Enabled && Math.Abs(settings.CaloriesMultiplier - 1f) > 0.0001f;
            CaloriesMultiplier = settings.CaloriesMultiplier;
            CalorieModifierAppliedTo.Clear();

            if (settings.Enabled && Math.Abs(settings.ExperienceMultiplier - 1f) > 0.0001f)
            {
                ApplySkillExperienceTuning(settings.ExperienceMultiplier, context);
            }
        }

        public static void ApplyRuntimeCalorieModifier(GameObject gameObject)
        {
            if (!CaloriesEnabled || gameObject == null)
            {
                return;
            }

            var instanceId = gameObject.GetInstanceID();
            if (CalorieModifierAppliedTo.Contains(instanceId))
            {
                return;
            }

            var amount = Db.Get().Amounts.Calories;
            var attribute = amount.deltaAttribute;
            var attributeInstance = attribute.Lookup(gameObject);
            if (attributeInstance == null)
            {
                return;
            }

            var current = attributeInstance.GetTotalValue();
            if (Math.Abs(current) < 0.0001f)
            {
                return;
            }

            var vanillaDelta = -Math.Abs(TUNING.DUPLICANTSTATS.STANDARD.BaseStats.CALORIES_BURNED_PER_SECOND);
            var target = DuplicantBalanceCalculator.ApplyCaloriesMultiplier(vanillaDelta, CaloriesMultiplier);
            var additional = DuplicantBalanceCalculator.CalculateMissingCalorieDelta(current, vanillaDelta, CaloriesMultiplier);
            if (Math.Abs(additional) < 0.0001f)
            {
                CalorieModifierAppliedTo.Add(instanceId);
                logger.Info(
                    "duplicant_calorie_modifier_skipped",
                    "Runtime calorie burn modifier was skipped because current burn is already at or above target.",
                    "current", current.ToString("0.###"),
                    "target", target.ToString("0.###"),
                    "multiplier", CaloriesMultiplier.ToString("0.###"));
                return;
            }

            attributeInstance.Add(new Klei.AI.AttributeModifier(
                attribute.Id,
                additional,
                "Hardcore Systems",
                false,
                false,
                true));
            CalorieModifierAppliedTo.Add(instanceId);
            logger.Info(
                "duplicant_calorie_modifier_applied",
                "Runtime calorie burn modifier was applied.",
                "current", current.ToString("0.###"),
                "target", target.ToString("0.###"),
                "additional", additional.ToString("0.###"),
                "multiplier", CaloriesMultiplier.ToString("0.###"));
        }

        private static void ApplySkillExperienceTuning(float multiplier, ModContext context)
        {
            if (experienceApplied)
            {
                return;
            }

            var type = typeof(TUNING.SKILLS);

            foreach (var fieldName in SkillExperienceFields)
            {
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
                if (field == null || field.FieldType != typeof(float))
                {
                    context.Logger.Warning("skill_experience_field_missing", "Skill experience tuning field was not found.", "field", fieldName);
                    continue;
                }

                var original = (float)field.GetValue(null);
                OriginalSkillExperience[fieldName] = original;
                field.SetValue(null, DuplicantBalanceCalculator.ApplyExperienceMultiplier(original, multiplier));
            }

            experienceApplied = true;
            context.Logger.Info("skill_experience_tuning_applied", "Skill experience gain tuning was applied.", "multiplier", multiplier.ToString("0.###"));
        }
    }
}
