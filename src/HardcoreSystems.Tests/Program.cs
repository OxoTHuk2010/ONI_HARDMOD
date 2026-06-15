using System;
using HardcoreSystems.Configuration;
using HardcoreSystems.Modules.DiseaseEffects;
using HardcoreSystems.Modules.DuplicantBalance;
using HardcoreSystems.Modules.ElectricalNetworks;
using HardcoreSystems.Modules.GeneratorEfficiency;
using HardcoreSystems.Modules.IndustrialHeat;
using HardcoreSystems.Modules.MiningYield;

namespace HardcoreSystems.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            TestOffPresetIsDisabled();
            TestHardPresetValues();
            TestValidationRejectsInvalidNumbers();
            TestMiningYieldCalculator();
            TestMiningYieldDigTracker();
            TestDuplicantBalanceCalculator();
            TestDuplicantRuntimeCalorieDelta();
            TestDiseasePenaltyCalculator();
            TestGeneratorEfficiencyCalculator();
            TestIndustrialHeatCalculator();
            TestElectricalNetworksCalculators();
            Console.WriteLine("All tests passed.");
            return 0;
        }

        private static void TestOffPresetIsDisabled()
        {
            var settings = PresetFactory.Create(DifficultyPreset.Off);
            Assert(!settings.Mining.Enabled, "Off disables mining");
            Assert(!settings.Duplicants.Enabled, "Off disables duplicant changes");
            Assert(SettingsValidator.Validate(settings).IsValid, "Off preset validates");
        }

        private static void TestHardPresetValues()
        {
            var settings = PresetFactory.Create(DifficultyPreset.Hard);
            Assert(settings.Mining.Enabled, "Hard enables mining module config");
            AssertClose(0.25f, settings.Mining.YieldMultiplier, "Hard mining yield");
            AssertClose(0.60f, settings.Duplicants.ExperienceMultiplier, "Hard XP");
            AssertClose(1.25f, settings.Duplicants.CaloriesMultiplier, "Hard calories");
            AssertClose(0.90f, settings.Power.GeneratorEfficiency, "Hard generator efficiency");
            Assert(SettingsValidator.Validate(settings).IsValid, "Hard preset validates");
        }

        private static void TestValidationRejectsInvalidNumbers()
        {
            var settings = PresetFactory.Create(DifficultyPreset.Custom);
            settings.Mining.YieldMultiplier = float.NaN;
            Assert(!SettingsValidator.Validate(settings).IsValid, "NaN mining yield is invalid");
            settings.Mining.YieldMultiplier = 2f;
            Assert(!SettingsValidator.Validate(settings).IsValid, "Out of range mining yield is invalid");
        }

        private static void TestMiningYieldCalculator()
        {
            AssertClose(25f, MiningYieldCalculator.Apply(100f, 0.25f), "Mining yield applies multiplier");
            AssertClose(100f, MiningYieldCalculator.Apply(100f, 1f), "Mining yield preserves vanilla at 1x");
            AssertClose(0f, MiningYieldCalculator.Apply(0f, 0.25f), "Mining yield preserves zero");
        }

        private static void TestMiningYieldDigTracker()
        {
            var tracker = new MiningYieldDigTracker();
            tracker.RecordDigCell(42);
            Assert(tracker.TryConsumeDigCell(42), "Recorded dig cell is consumed");
            Assert(!tracker.TryConsumeDigCell(42), "Dig cell is consumed only once");
            Assert(!tracker.TryConsumeDigCell(43), "Unrelated cell is not consumed");
        }

        private static void TestDuplicantBalanceCalculator()
        {
            AssertClose(30f, DuplicantBalanceCalculator.ApplyExperienceMultiplier(100f, 0.30f), "XP multiplier applies");
            AssertClose(150f, DuplicantBalanceCalculator.ApplyCaloriesMultiplier(100f, 1.50f), "Calorie multiplier applies");
            AssertClose(-5000f, DuplicantBalanceCalculator.ApplyCaloriesMultiplier(-1000f, 5f), "Calorie multiplier scales burn rate");
            AssertClose(0f, DuplicantBalanceCalculator.ApplyCaloriesMultiplier(0f, 2f), "Calorie multiplier preserves zero");
        }

        private static void TestDuplicantRuntimeCalorieDelta()
        {
            var current = -1000f;
            var vanilla = -1000f;
            AssertClose(-4000f, DuplicantBalanceCalculator.CalculateMissingCalorieDelta(current, vanilla, 5f), "Runtime calorie modifier adds missing burn delta");
            AssertClose(0f, DuplicantBalanceCalculator.CalculateMissingCalorieDelta(-5000f, vanilla, 5f), "Runtime calorie modifier does not stack above target");
            AssertClose(0f, DuplicantBalanceCalculator.CalculateMissingCalorieDelta(-25000f, vanilla, 5f), "Runtime calorie modifier does not worsen an over-stacked save");
        }

        private static void TestDiseasePenaltyCalculator()
        {
            AssertClose(-0.30f, DiseasePenaltyCalculator.ProductivityMultiplier(0.30f), "Food poisoning productivity penalty");
            Assert(DiseasePenaltyCalculator.SlimelungMoralePenalty(0.10f) == -1, "VanillaPlus morale penalty");
            Assert(DiseasePenaltyCalculator.SlimelungMoralePenalty(0.30f) == -3, "Hard morale penalty");
            Assert(DiseasePenaltyCalculator.SlimelungMoralePenalty(0.50f) == -5, "Extreme morale penalty");
            Assert(DiseasePenaltyCalculator.SlimelungMoralePenalty(0.80f) == -8, "Insane morale penalty");
        }

        private static void TestGeneratorEfficiencyCalculator()
        {
            AssertClose(450f, GeneratorEfficiencyCalculator.Apply(500f, 0.90f), "Generator efficiency scales output");
            AssertClose(500f, GeneratorEfficiencyCalculator.Apply(500f, 1.50f), "Generator efficiency clamps high");
            AssertClose(50f, GeneratorEfficiencyCalculator.Apply(500f, 0.01f), "Generator efficiency clamps low");
        }

        private static void TestIndustrialHeatCalculator()
        {
            AssertClose(0.001f, IndustrialHeatCalculator.CalculateTemperatureDelta(1000f, 1f, 0.2f, 400f, 0.5f, 2f), "Industrial heat delta");
            AssertClose(0f, IndustrialHeatCalculator.CalculateTemperatureDelta(0f, 1f, 0.2f, 400f, 0.5f, 2f), "Industrial heat skips zero watts");
            AssertClose(2f, IndustrialHeatCalculator.CalculateTemperatureDelta(1000000f, 10f, 1f, 1f, 0.1f, 2f), "Industrial heat clamps per tick");
            AssertClose(4f, IndustrialHeatCalculator.ScaleOperatingKilowatts(2f, 2f), "Industrial heat scales ONI operating heat");
            AssertClose(2f, IndustrialHeatCalculator.ScaleOperatingKilowatts(2f, 0f), "Industrial heat preserves value for disabled multiplier");
            AssertClose(2f, IndustrialHeatCalculator.CalculatePumpFallbackKilowatts(240f), "Gas pump fallback heat");
            AssertClose(0.5f, IndustrialHeatCalculator.CalculatePumpFallbackKilowatts(60f), "Mini gas pump fallback heat");
        }

        private static void TestElectricalNetworksCalculators()
        {
            var copperCell = ElectricalLossCalculator.CalculateCellResistanceOhms(1f, 0.75f, 293.15f, 1000f);
            var hotCopperCell = ElectricalLossCalculator.CalculateCellResistanceOhms(1f, 0.75f, 393.15f, 1000f);
            AssertClose(0.0015f, copperCell, "Cell resistance uses material factor");
            AssertClose(0.0021f, hotCopperCell, "Cell resistance uses temperature coefficient");
            AssertClose(1.5f, ElectricalLossCalculator.CalculateOhmicLossWatts(1000f, copperCell), "Ohmic loss uses current squared");
            AssertClose(0.985f, ElectricalLossCalculator.CalculateAvailability(1000f, 15f), "Availability is delivered watt ratio");
            Assert(ElectricalLossCalculator.ShouldPowerConsumer(0.75f, 0, 10), "Brownout duty cycle powers early slots");
            Assert(!ElectricalLossCalculator.ShouldPowerConsumer(0.75f, 9, 10), "Brownout duty cycle cuts late slots");
            Assert(!ElectricalLossCalculator.ShouldPowerConsumer(0.49f, 0, 10), "Below half power stays off");
            AssertClose(300f, ElectricalLossCalculator.CalculateOverloadHeatWatts(2000f, 1000f, 1f), "Overload heat applies above safe wattage");
            AssertClose(525000f, ElectricalLossCalculator.CalculateShortCircuitImpulseWatts(1), "Short circuit impulse is intentionally large");
            AssertClose(0.0125f, ElectricalLossCalculator.CalculateTemperatureDelta(1000f, 1f, 0.2f, 400f, 2f), "Electrical wire heat delta");
            AssertClose(0f, ElectricalLossCalculator.CalculateCellResistanceOhms(0f, 1f, 293.15f, 1000f), "Zero resistance disables loss");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void AssertClose(float expected, float actual, string message)
        {
            if (Math.Abs(expected - actual) > 0.0001f)
            {
                throw new InvalidOperationException(message + ": expected " + expected + " actual " + actual);
            }
        }
    }
}
