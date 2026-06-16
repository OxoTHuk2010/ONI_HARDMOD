using HarmonyLib;
using HardcoreSystems.Bootstrap;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Modules.PowerGeneration
{
    public sealed class PowerGenerationModule : IGameplayModule
    {
        public string Id
        {
            get { return "PowerGeneration"; }
        }

        public bool IsEnabled(ModContext context)
        {
            return context.Settings.Power.GeneratorRebalanceEnabled;
        }

        public void Initialize(ModContext context)
        {
            PowerGenerationRuntime.Configure(context);
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(Generator), "WattageRating"),
                    null,
                    new HarmonyMethod(typeof(PowerGenerationPatches), "WattageRatingPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(StructureTemperaturePayload), "OperatingKilowatts"),
                    null,
                    new HarmonyMethod(typeof(PowerGenerationPatches), "OperatingKilowattsPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(StructureTemperaturePayload), "ExhaustKilowatts"),
                    null,
                    new HarmonyMethod(typeof(PowerGenerationPatches), "ExhaustKilowattsPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.PropertyGetter(typeof(StructureTemperaturePayload), "TotalEnergyProducedKW"),
                    null,
                    new HarmonyMethod(typeof(PowerGenerationPatches), "TotalEnergyProducedPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(EnergyGenerator), "EnergySim200ms", new[] { typeof(float) }),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EnergyGeneratorSimPrefix"),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EnergyGeneratorSimPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(EnergyGenerator), "Emit", new[] { typeof(EnergyGenerator.OutputItem), typeof(float), typeof(PrimaryElement) }),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EmitPrefix"),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EmitPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(ManualGenerator), "EnergySim200ms", new[] { typeof(float) }),
                    null,
                    new HarmonyMethod(typeof(PowerGenerationPatches), "ManualGeneratorSimPostfix"),
                    Id));

            ModulePatchReporter.Log(
                context.Logger,
                PatchGuard.TryPatch(
                    harmony,
                    AccessTools.Method(typeof(Building), "EffectDescriptors", new[] { typeof(BuildingDef) }),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EffectDescriptorsPrefix"),
                    new HarmonyMethod(typeof(PowerGenerationPatches), "EffectDescriptorsPostfix"),
                    Id));
        }

        public void OnGameStarted(ModContext context) { }

        public void OnGameLoaded(ModContext context) { }

        public void OnSettingsChanged(ModContext context, ModSettings previous, ModSettings current)
        {
            PowerGenerationRuntime.Configure(context);
        }

        public void Shutdown(ModContext context) { }
    }

    internal static class PowerGenerationPatches
    {
        public static void WattageRatingPostfix(Generator __instance, ref float __result)
        {
            PowerGenerationRuntime.ApplyToWattage(__instance, ref __result);
        }

        public static void OperatingKilowattsPostfix(StructureTemperaturePayload __instance, ref float __result)
        {
            PowerGenerationRuntime.ApplyToOperatingKilowatts(__instance, ref __result);
        }

        public static void EnergyGeneratorSimPrefix(EnergyGenerator __instance, out FuelThermalState __state)
        {
            __state = PowerGenerationRuntime.CaptureFuelThermalState(__instance);
        }

        public static void EnergyGeneratorSimPostfix(EnergyGenerator __instance, float dt, FuelThermalState __state)
        {
            PowerGenerationRuntime.ApplyRuntimeHeat(__instance, dt, __state);
        }

        public static void ManualGeneratorSimPostfix(ManualGenerator __instance, float dt)
        {
            PowerGenerationRuntime.ApplyRuntimeHeat(__instance, dt, null);
        }

        public static void EffectDescriptorsPrefix(BuildingDef def, out PowerGenerationDescriptorState __state)
        {
            __state = null;
            if (def == null)
            {
                return;
            }

            __state = new PowerGenerationDescriptorState(
                def.GeneratorWattageRating,
                def.SelfHeatKilowattsWhenActive,
                def.ExhaustKilowattsWhenActive);

            def.GeneratorWattageRating = PowerGenerationRuntime.ApplyToDescriptorWattage(def, def.GeneratorWattageRating);
            def.SelfHeatKilowattsWhenActive = PowerGenerationRuntime.ApplyToDescriptorSelfHeat(def, def.SelfHeatKilowattsWhenActive);
            def.ExhaustKilowattsWhenActive = PowerGenerationRuntime.ApplyToDescriptorExhaustHeat(def, def.ExhaustKilowattsWhenActive);
        }

        public static void EffectDescriptorsPostfix(BuildingDef def, PowerGenerationDescriptorState __state)
        {
            if (def != null && __state != null)
            {
                def.GeneratorWattageRating = __state.GeneratorWattageRating;
                def.SelfHeatKilowattsWhenActive = __state.SelfHeatKilowattsWhenActive;
                def.ExhaustKilowattsWhenActive = __state.ExhaustKilowattsWhenActive;
            }
        }

        public static void ExhaustKilowattsPostfix(StructureTemperaturePayload __instance, ref float __result)
        {
            PowerGenerationRuntime.ApplyToExhaustKilowatts(__instance, ref __result);
        }

        public static void TotalEnergyProducedPostfix(StructureTemperaturePayload __instance, ref float __result)
        {
            PowerGenerationRuntime.ApplyToTotalEnergyProduced(__instance, ref __result);
        }

        public static void EmitPrefix(EnergyGenerator __instance, ref EnergyGenerator.OutputItem output, float dt, PrimaryElement root_pe)
        {
            PowerGenerationRuntime.ApplyToGeneratorOutputTemperature(__instance, ref output, dt, root_pe);
        }

        public static void EmitPostfix(EnergyGenerator __instance, EnergyGenerator.OutputItem output)
        {
            PowerGenerationRuntime.ApplyToStoredGeneratorOutputTemperature(__instance, output);
        }
    }

    internal sealed class PowerGenerationDescriptorState
    {
        public PowerGenerationDescriptorState(float generatorWattageRating, float selfHeatKilowattsWhenActive, float exhaustKilowattsWhenActive)
        {
            GeneratorWattageRating = generatorWattageRating;
            SelfHeatKilowattsWhenActive = selfHeatKilowattsWhenActive;
            ExhaustKilowattsWhenActive = exhaustKilowattsWhenActive;
        }

        public float GeneratorWattageRating { get; private set; }
        public float SelfHeatKilowattsWhenActive { get; private set; }
        public float ExhaustKilowattsWhenActive { get; private set; }
    }
}
