using System;
using System.Collections.Generic;
using HardcoreSystems.Diagnostics;
using HardcoreSystems.Modules.Thermal;
using UnityEngine;

namespace HardcoreSystems.Modules.PowerGeneration
{
    internal static class PowerGenerationRuntime
    {
        private static readonly Dictionary<int, FuelThermalState> ActiveFuelStates = new Dictionary<int, FuelThermalState>();
        private static ModLogger logger;

        public static bool Enabled { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            Enabled = context.Settings.Power.GeneratorRebalanceEnabled;
        }

        public static void ApplyToWattage(Generator generator, ref float watts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                PowerGenerationProfile profile;
                if (!Enabled || !TryGetProfile(generator, out profile))
                {
                    DiagnosticsRuntime.Record("PowerGeneration", start, 0, 1, 0);
                    return;
                }

                watts = profile.Wattage;
                DiagnosticsRuntime.Record("PowerGeneration", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("PowerGeneration", start, 0, 0, 1);
                logger.RateLimitedWarning("power_generation_wattage_failed", "power_generation_wattage_failed", "Power generation wattage patch failed and was skipped.");
            }
        }

        public static void ApplyToOperatingKilowatts(StructureTemperaturePayload payload, ref float kilowatts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                PowerGenerationProfile profile;
                if (!Enabled || payload.building == null || !PowerGenerationProfileRegistry.TryGet(payload.building.Def.PrefabID, out profile))
                {
                    DiagnosticsRuntime.Record("PowerGeneration", start, 0, 1, 0);
                    return;
                }

                kilowatts = 0f;
                DiagnosticsRuntime.Record("PowerGeneration", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("PowerGeneration", start, 0, 0, 1);
                logger.RateLimitedWarning("power_generation_heat_failed", "power_generation_heat_failed", "Power generation heat patch failed and was skipped.");
            }
        }

        public static void ApplyToExhaustKilowatts(StructureTemperaturePayload payload, ref float kilowatts)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                PowerGenerationProfile profile;
                if (!Enabled || payload.building == null || !PowerGenerationProfileRegistry.TryGet(payload.building.Def.PrefabID, out profile))
                {
                    DiagnosticsRuntime.Record("PowerGenerationExhaust", start, 0, 1, 0);
                    return;
                }

                kilowatts = 0f;
                DiagnosticsRuntime.Record("PowerGenerationExhaust", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("PowerGenerationExhaust", start, 0, 0, 1);
                logger.RateLimitedWarning("power_generation_exhaust_heat_failed", "power_generation_exhaust_heat_failed", "Power generation exhaust heat suppression failed and was skipped.");
            }
        }

        public static FuelThermalState CaptureFuelThermalState(EnergyGenerator generator)
        {
            try
            {
                PowerGenerationProfile profile;
                if (!Enabled || generator == null || !TryGetProfile(generator.gameObject, out profile) || !profile.AccountsFuelTemperature)
                {
                    return null;
                }

                var storage = generator.GetComponent<Storage>();
                if (storage == null || generator.formula.inputs == null || generator.formula.inputs.Length == 0)
                {
                    return null;
                }

                var state = new FuelThermalState();
                for (var i = 0; i < generator.formula.inputs.Length; i++)
                {
                    var input = generator.formula.inputs[i];
                    var massBefore = storage.GetMassAvailable(input.tag);
                    if (massBefore <= 0f)
                    {
                        continue;
                    }

                    float temperature;
                    float specificHeatCapacity;
                    if (!TryGetStoredFuelThermalData(storage, input.tag, out temperature, out specificHeatCapacity))
                    {
                        continue;
                    }

                    state.Samples.Add(new FuelThermalSample(input.tag, massBefore, temperature, specificHeatCapacity));
                }

                if (state.HasSamples)
                {
                    ActiveFuelStates[generator.gameObject.GetInstanceID()] = state;
                    return state;
                }

                ActiveFuelStates.Remove(generator.gameObject.GetInstanceID());
                return null;
            }
            catch (Exception)
            {
                logger.RateLimitedWarning("power_generation_fuel_state_failed", "power_generation_fuel_state_failed", "Power generator fuel thermal state capture failed and was skipped.");
                return null;
            }
        }

        public static void ApplyRuntimeHeat(Component component, float deltaTimeSeconds, FuelThermalState fuelState)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                PowerGenerationProfile profile;
                if (!Enabled || component == null || deltaTimeSeconds <= 0f || !TryGetProfile(component.gameObject, out profile))
                {
                    DiagnosticsRuntime.Record("PowerGenerationRuntimeHeat", start, 0, 1, 0);
                    return;
                }

                var operational = component.GetComponent<Operational>();
                if (operational != null && !operational.IsActive)
                {
                    DiagnosticsRuntime.Record("PowerGenerationRuntimeHeat", start, 0, 1, 0);
                    return;
                }

                var energyDtu = PowerGenerationHeatCalculator.CalculateProfileHeatDtu(profile.BodyHeatKilowatts, deltaTimeSeconds);
                energyDtu += CalculateFuelSurplusHeatDtu(component, fuelState);
                if (energyDtu <= 0.0)
                {
                    DiagnosticsRuntime.Record("PowerGenerationRuntimeHeat", start, 0, 1, 0);
                    return;
                }

                if (!BuildingThermalAdapter.TryAddEnergy(component.gameObject, energyDtu, 1f, 10000f))
                {
                    DiagnosticsRuntime.Record("PowerGenerationRuntimeHeat", start, 0, 1, 0);
                    logger.RateLimitedWarning("power_generation_runtime_heat_no_sim_handle", "power_generation_runtime_heat_no_sim_handle", "Power generator runtime heat skipped because the building thermal sim handle is not available.");
                    return;
                }

                DiagnosticsRuntime.Record("PowerGenerationRuntimeHeat", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("PowerGenerationRuntimeHeat", start, 0, 0, 1);
                logger.RateLimitedWarning("power_generation_runtime_heat_failed", "power_generation_runtime_heat_failed", "Power generator runtime heat patch failed and was skipped.");
            }
            finally
            {
                if (component != null)
                {
                    ActiveFuelStates.Remove(component.gameObject.GetInstanceID());
                }
            }
        }

        public static float ApplyToDescriptorWattage(BuildingDef def, float watts)
        {
            PowerGenerationProfile profile;
            if (!Enabled || def == null || !PowerGenerationProfileRegistry.TryGet(def.PrefabID, out profile))
            {
                return watts;
            }

            return profile.Wattage;
        }

        public static float ApplyToDescriptorSelfHeat(BuildingDef def, float selfHeatKilowatts)
        {
            PowerGenerationProfile profile;
            if (!Enabled || def == null || !PowerGenerationProfileRegistry.TryGet(def.PrefabID, out profile))
            {
                return selfHeatKilowatts;
            }

            return profile.BodyHeatKilowatts;
        }

        public static float ApplyToDescriptorExhaustHeat(BuildingDef def, float exhaustHeatKilowatts)
        {
            PowerGenerationProfile profile;
            if (!Enabled || def == null || !PowerGenerationProfileRegistry.TryGet(def.PrefabID, out profile))
            {
                return exhaustHeatKilowatts;
            }

            return 0f;
        }

        public static void ApplyToTotalEnergyProduced(StructureTemperaturePayload payload, ref float kilowatts)
        {
            PowerGenerationProfile profile;
            if (!Enabled
                || payload.building == null
                || payload.operational == null
                || !payload.operational.IsActive
                || !PowerGenerationProfileRegistry.TryGet(payload.building.Def.PrefabID, out profile))
            {
                return;
            }

            kilowatts = profile.BodyHeatKilowatts;
        }

        public static void ApplyToGeneratorOutputTemperature(EnergyGenerator generator, ref EnergyGenerator.OutputItem output, float deltaTimeSeconds, PrimaryElement rootElement)
        {
            try
            {
                if (!Enabled || generator == null || deltaTimeSeconds <= 0f)
                {
                    return;
                }

                PowerGenerationProfile profile;
                if (!TryGetProfile(generator.gameObject, out profile) || !profile.AccountsFuelTemperature)
                {
                    return;
                }

                FuelThermalState state;
                if (!ActiveFuelStates.TryGetValue(generator.gameObject.GetInstanceID(), out state) || state == null || !state.HasSamples)
                {
                    return;
                }

                var element = ElementLoader.FindElementByHash(output.element);
                if (element == null || (!element.IsGas && !element.IsLiquid))
                {
                    return;
                }

                if (output.store)
                {
                    return;
                }

                var targetTemperature = CalculateSafeOutputTemperature(element, state.MaximumTemperatureKelvin);
                var currentTemperature = rootElement == null
                    ? output.minTemperature
                    : Math.Max(rootElement.Temperature, output.minTemperature);
                if (targetTemperature <= currentTemperature)
                {
                    return;
                }

                var outputMass = output.creationRate * deltaTimeSeconds;
                var outputEnergyDtu = PowerGenerationHeatCalculator.CalculateOutputHeatDtu(
                    outputMass,
                    element.specificHeatCapacity,
                    currentTemperature,
                    targetTemperature);
                state.AddOutputEnergyAssigned(outputEnergyDtu);
                output.minTemperature = targetTemperature;
            }
            catch (Exception)
            {
                logger.RateLimitedWarning("power_generation_output_temperature_failed", "power_generation_output_temperature_failed", "Power generator output temperature patch failed and was skipped.");
            }
        }

        public static void ApplyToStoredGeneratorOutputTemperature(EnergyGenerator generator, EnergyGenerator.OutputItem output)
        {
            try
            {
                if (!Enabled || generator == null || !output.store)
                {
                    return;
                }

                PowerGenerationProfile profile;
                if (!TryGetProfile(generator.gameObject, out profile) || !profile.AccountsFuelTemperature)
                {
                    return;
                }

                FuelThermalState state;
                if (!ActiveFuelStates.TryGetValue(generator.gameObject.GetInstanceID(), out state) || state == null || !state.HasSamples)
                {
                    return;
                }

                var element = ElementLoader.FindElementByHash(output.element);
                if (element == null || (!element.IsGas && !element.IsLiquid))
                {
                    return;
                }

                var storage = generator.GetComponent<Storage>();
                if (storage == null)
                {
                    return;
                }

                var targetTemperature = CalculateSafeOutputTemperature(element, state.MaximumTemperatureKelvin);
                if (targetTemperature <= 0f)
                {
                    return;
                }

                var items = storage.GetItems();
                if (items == null || items.Count == 0)
                {
                    return;
                }

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if (item == null)
                    {
                        continue;
                    }

                    var primary = item.GetComponent<PrimaryElement>();
                    if (primary == null || primary.Element == null || primary.Element.id != output.element || primary.Mass <= 0f)
                    {
                        continue;
                    }

                    if (targetTemperature <= primary.Temperature)
                    {
                        continue;
                    }

                    var outputEnergyDtu = PowerGenerationHeatCalculator.CalculateOutputHeatDtu(
                        primary.Mass,
                        primary.Element.specificHeatCapacity,
                        primary.Temperature,
                        targetTemperature);
                    state.AddOutputEnergyAssigned(outputEnergyDtu);
                    primary.Temperature = targetTemperature;
                }
            }
            catch (Exception)
            {
                logger.RateLimitedWarning("power_generation_stored_output_temperature_failed", "power_generation_stored_output_temperature_failed", "Power generator stored output temperature patch failed and was skipped.");
            }
        }

        public static bool ShouldOwnGeneratorHeat(BuildingDef def)
        {
            return Enabled && PowerGenerationProfileRegistry.IsProfiledGenerator(def);
        }

        private static double CalculateFuelSurplusHeatDtu(Component component, FuelThermalState state)
        {
            if (component == null || state == null || !state.HasSamples)
            {
                return 0.0;
            }

            var storage = component.GetComponent<Storage>();
            var body = component.GetComponent<PrimaryElement>();
            if (storage == null || body == null)
            {
                return 0.0;
            }

            double energyDtu = 0.0;
            for (var i = 0; i < state.Samples.Count; i++)
            {
                var sample = state.Samples[i];
                var massAfter = storage.GetMassAvailable(sample.Tag);
                var consumed = Math.Max(0.0, sample.MassBeforeKilograms - massAfter);
                energyDtu += PowerGenerationHeatCalculator.CalculateFuelSurplusHeatDtu(
                    consumed,
                    sample.SpecificHeatCapacity,
                    sample.TemperatureKelvin,
                    body.Temperature);
            }

            return Math.Max(0.0, energyDtu - state.OutputEnergyAssignedDtu);
        }

        private static float CalculateSafeOutputTemperature(Element outputElement, float fuelTemperatureKelvin)
        {
            if (outputElement == null || fuelTemperatureKelvin <= 0f)
            {
                return 0f;
            }

            if (!outputElement.IsLiquid)
            {
                return fuelTemperatureKelvin;
            }

            var phaseChangeTemperature = outputElement.highTemp;
            if (float.IsNaN(phaseChangeTemperature) || float.IsInfinity(phaseChangeTemperature) || phaseChangeTemperature <= 1f)
            {
                return fuelTemperatureKelvin;
            }

            return Math.Min(fuelTemperatureKelvin, phaseChangeTemperature - 1f);
        }

        private static bool TryGetStoredFuelThermalData(Storage storage, Tag tag, out float temperatureKelvin, out float specificHeatCapacity)
        {
            temperatureKelvin = 0f;
            specificHeatCapacity = 0f;

            var items = storage.GetItems();
            if (items == null || items.Count == 0)
            {
                return false;
            }

            double totalMass = 0.0;
            double weightedTemperature = 0.0;
            float shc = 0f;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    continue;
                }

                var primary = item.GetComponent<PrimaryElement>();
                if (primary == null || primary.Element == null || primary.Mass <= 0f || !HasMatchingTag(item, primary, tag))
                {
                    continue;
                }

                totalMass += primary.Mass;
                weightedTemperature += primary.Mass * primary.Temperature;
                shc = primary.Element.specificHeatCapacity;
            }

            if (totalMass <= 0.0 || shc <= 0f)
            {
                return false;
            }

            temperatureKelvin = (float)(weightedTemperature / totalMass);
            specificHeatCapacity = shc;
            return true;
        }

        private static bool HasMatchingTag(GameObject item, PrimaryElement primary, Tag tag)
        {
            if (primary.Element.tag == tag)
            {
                return true;
            }

            var prefabId = item.GetComponent<KPrefabID>();
            return prefabId != null && prefabId.HasTag(tag);
        }

        private static bool TryGetProfile(Generator generator, out PowerGenerationProfile profile)
        {
            profile = null;
            if (generator == null)
            {
                return false;
            }

            return TryGetProfile(generator.gameObject, out profile);
        }

        private static bool TryGetProfile(GameObject gameObject, out PowerGenerationProfile profile)
        {
            profile = null;
            if (gameObject == null)
            {
                return false;
            }

            var building = gameObject.GetComponent<Building>();
            return building != null && building.Def != null && PowerGenerationProfileRegistry.TryGet(building.Def.PrefabID, out profile);
        }
    }
}
