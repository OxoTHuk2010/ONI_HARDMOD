using System;
using HardcoreSystems.Configuration;
using HardcoreSystems.Modules.DiseaseEffects;
using HardcoreSystems.Modules.DuplicantBalance;
using HardcoreSystems.Modules.ElectricalOverloadThermalDamage;
using HardcoreSystems.Modules.IndustrialHeat;
using HardcoreSystems.Modules.MiningYield;
using HardcoreSystems.Modules.PowerGeneration;
using HardcoreSystems.Modules.SolarGeneration;

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
            TestPowerGenerationProfiles();
            TestPowerGenerationHeatCalculator();
            TestIndustrialHeatCalculator();
            TestSolarHeatCalculator();
            TestElectricalOverloadHeatCalculator();
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
            Assert(settings.Power.GeneratorRebalanceEnabled, "Hard enables v0.4 generator rebalance");
            Assert(settings.Power.SolarPanelGenerationHeatEnabled, "Hard enables solar generation heat");
            Assert(settings.Power.OverloadHeatEnabled, "Hard enables overload wire heating");
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

        private static void TestPowerGenerationHeatCalculator()
        {
            AssertClose(8000.0, PowerGenerationHeatCalculator.CalculateProfileHeatDtu(40.0, 0.2), "Profile heat converts kDTU/s to DTU per tick");
            AssertClose(0.0, PowerGenerationHeatCalculator.CalculateProfileHeatDtu(0.0, 0.2), "Profile heat skips zero heat");
            AssertClose(0.0, PowerGenerationHeatCalculator.CalculateProfileHeatDtu(40.0, 0.0), "Profile heat skips zero dt");

            AssertClose(134400.0, PowerGenerationHeatCalculator.CalculateFuelSurplusHeatDtu(0.1, 2.4, 873.15, 313.15), "Fuel surplus heat uses kg to grams conversion");
            AssertClose(0.0, PowerGenerationHeatCalculator.CalculateFuelSurplusHeatDtu(0.1, 2.4, 300.0, 313.15), "Fuel surplus heat does not cool body");
            AssertClose(0.0, PowerGenerationHeatCalculator.CalculateFuelSurplusHeatDtu(0.0, 2.4, 873.15, 313.15), "Fuel surplus heat skips zero mass");
        }

        private static void TestPowerGenerationProfiles()
        {
            PowerGenerationProfile profile;
            Assert(PowerGenerationProfileRegistry.TryGet("Generator", out profile), "Coal generator profile exists");
            AssertClose(600f, profile.Wattage, "Coal generator wattage");
            AssertClose(40f, profile.BodyHeatKilowatts, "Coal generator heat");

            Assert(PowerGenerationProfileRegistry.TryGet("HydrogenGenerator", out profile), "Hydrogen generator profile exists");
            AssertClose(800f, profile.Wattage, "Hydrogen generator wattage");
            AssertClose(32f, profile.BodyHeatKilowatts, "Hydrogen generator heat");

            Assert(PowerGenerationProfileRegistry.TryGet("PetroleumGenerator", out profile), "Petroleum generator profile exists");
            AssertClose(2000f, profile.Wattage, "Petroleum generator wattage");
            AssertClose(50f, profile.BodyHeatKilowatts, "Petroleum generator heat");

            Assert(!PowerGenerationProfileRegistry.TryGet("SteamTurbine2", out profile), "Steam turbine remains excluded");
            Assert(!PowerGenerationProfileRegistry.TryGet("SolarPanel", out profile), "Solar panel remains excluded from fuel generator profiles");
        }

        private static void TestSolarHeatCalculator()
        {
            AssertClose(0.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(0.0, 380.0, 5000.0), "Solar heat at zero power");
            AssertClose(1250.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(95.0, 380.0, 5000.0), "Solar heat at 95W");
            AssertClose(2500.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(190.0, 380.0, 5000.0), "Solar heat at 190W");
            AssertClose(3750.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(285.0, 380.0, 5000.0), "Solar heat at 285W");
            AssertClose(5000.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(380.0, 380.0, 5000.0), "Solar heat at 380W");
            AssertClose(5000.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(500.0, 380.0, 5000.0), "Solar heat clamps above max");
            AssertClose(0.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(-1.0, 380.0, 5000.0), "Solar heat clamps negative power");
            AssertClose(0.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(double.NaN, 380.0, 5000.0), "Solar heat ignores NaN");
            AssertClose(0.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(double.PositiveInfinity, 380.0, 5000.0), "Solar heat ignores infinity");
            AssertClose(0.0, SolarHeatCalculator.CalculateSolarHeatDtuPerSecond(100.0, 0.0, 5000.0), "Solar heat ignores zero max power");
            AssertClose(1000.0, SolarHeatCalculator.CalculateEnergyDtu(5000.0, 0.2), "Solar heat scales by delta time");
        }

        private static void TestElectricalOverloadHeatCalculator()
        {
            var copperMeltingKelvin = 1357.77;
            var coldCopper = OverloadHeatCalculator.CalculateOverloadHeat(300.0, copperMeltingKelvin, 100.0, 0.385);
            Assert(coldCopper.IsValid, "Cold copper overload heat is valid");
            Assert(coldCopper.ShouldApply, "Cold copper overload heat applies");
            AssertClose(copperMeltingKelvin * 0.90, coldCopper.TargetTemperatureKelvin, "Overload target is 90 percent of melting point in Kelvin");
            AssertClose(100.0 * 0.385 * (copperMeltingKelvin * 0.90 - 300.0), coldCopper.RequiredEnergyDtu, "Overload heat uses mass and SHC");

            var fromKilograms = OverloadHeatCalculator.CalculateOverloadHeatFromKilograms(300.0, copperMeltingKelvin, 0.1, 0.385);
            AssertClose(coldCopper.RequiredEnergyDtu, fromKilograms.RequiredEnergyDtu, "Runtime overload heat converts building mass kilograms to grams");

            var alreadyHot = OverloadHeatCalculator.CalculateOverloadHeat(copperMeltingKelvin, copperMeltingKelvin, 100.0, 0.385);
            Assert(alreadyHot.IsValid, "Hot wire overload heat is valid");
            Assert(!alreadyHot.ShouldApply, "Hot wire is not cooled");
            AssertClose(0.0, alreadyHot.RequiredEnergyDtu, "Hot wire receives no negative energy");

            Assert(!OverloadHeatCalculator.CalculateOverloadHeat(300.0, copperMeltingKelvin, 0.0, 0.385).IsValid, "Zero mass is invalid");
            Assert(!OverloadHeatCalculator.CalculateOverloadHeat(300.0, copperMeltingKelvin, 100.0, 0.0).IsValid, "Zero SHC is invalid");
            Assert(!OverloadHeatCalculator.CalculateOverloadHeat(300.0, 0.0, 100.0, 0.385).IsValid, "Invalid melting temperature is rejected");
            Assert(!OverloadHeatCalculator.CalculateOverloadHeat(double.NaN, copperMeltingKelvin, 100.0, 0.385).IsValid, "NaN is rejected");
            Assert(!OverloadHeatCalculator.CalculateOverloadHeat(double.PositiveInfinity, copperMeltingKelvin, 100.0, 0.385).IsValid, "Infinity is rejected");
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

        private static void AssertClose(double expected, double actual, string message)
        {
            if (Math.Abs(expected - actual) > 0.0001)
            {
                throw new InvalidOperationException(message + ": expected " + expected + " actual " + actual);
            }
        }
    }
}
