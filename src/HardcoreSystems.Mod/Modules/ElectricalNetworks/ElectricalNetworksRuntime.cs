using System;
using System.Collections.Generic;
using HardcoreSystems.Diagnostics;
using UnityEngine;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    internal static class ElectricalNetworksRuntime
    {
        private const int MaxWiresPerTick = 96;
        private const float MaxWireDeltaKelvinPerTick = 0.25f;

        private static readonly List<Wire> Wires = new List<Wire>();
        private static readonly HashSet<ushort> DrainedCircuits = new HashSet<ushort>();
        private static ModLogger logger;
        private static int nextWireIndex;

        public static bool ElectricalLossesEnabled { get; private set; }
        public static bool TransformerEfficiencyEnabled { get; private set; }
        public static bool OverloadHeatEnabled { get; private set; }
        public static float TransformerEfficiency { get; private set; }
        public static float WireResistanceMultiplier { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            var settings = context.Settings.Power;
            ElectricalLossesEnabled = settings.ElectricalLossesEnabled && settings.WireResistanceMultiplier > 0.0001f;
            TransformerEfficiencyEnabled = settings.TransformerEfficiencyEnabled && settings.TransformerEfficiency < 0.9999f;
            OverloadHeatEnabled = settings.OverloadHeatEnabled;
            TransformerEfficiency = settings.TransformerEfficiency;
            WireResistanceMultiplier = settings.WireResistanceMultiplier;
        }

        public static void RegisterWire(Wire wire)
        {
            if (wire == null || Wires.Contains(wire))
            {
                return;
            }

            Wires.Add(wire);
        }

        public static void UnregisterWire(Wire wire)
        {
            if (wire == null)
            {
                return;
            }

            Wires.Remove(wire);
            if (nextWireIndex >= Wires.Count)
            {
                nextWireIndex = 0;
            }
        }

        public static void ApplyTransformerEfficiency(float outputJoules, Battery inputBattery)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!TransformerEfficiencyEnabled || inputBattery == null || outputJoules <= 0f)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var extraInputJoules = TransformerEfficiencyCalculator.CalculateAdditionalInputJoules(outputJoules, TransformerEfficiency);
                if (extraInputJoules <= 0f)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                inputBattery.ConsumeEnergy(extraInputJoules);
                DiagnosticsRuntime.Record("ElectricalNetworks", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 0, 1);
                logger.RateLimitedWarning("transformer_efficiency_apply_failed", "transformer_efficiency_apply_failed", "Transformer efficiency patch failed and was skipped.");
            }
        }

        public static void SimulateCircuitTick(CircuitManager manager, float dt)
        {
            var start = DiagnosticsRuntime.Begin();
            var processed = 0;
            var skipped = 0;

            try
            {
                if (manager == null || dt <= 0f || Wires.Count == 0 || (!ElectricalLossesEnabled && !OverloadHeatEnabled))
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                DrainedCircuits.Clear();
                var budget = Math.Min(MaxWiresPerTick, Wires.Count);
                for (var i = 0; i < budget && Wires.Count > 0; i++)
                {
                    if (nextWireIndex >= Wires.Count)
                    {
                        nextWireIndex = 0;
                    }

                    var wire = Wires[nextWireIndex];
                    nextWireIndex++;

                    if (wire == null)
                    {
                        Wires.RemoveAt(nextWireIndex - 1);
                        nextWireIndex--;
                        skipped++;
                        continue;
                    }

                    if (ProcessWire(manager, wire, dt))
                    {
                        processed++;
                    }
                    else
                    {
                        skipped++;
                    }
                }

                DiagnosticsRuntime.Record("ElectricalNetworks", start, processed, skipped, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("ElectricalNetworks", start, processed, skipped, 1);
                logger.RateLimitedWarning("electrical_network_tick_failed", "electrical_network_tick_failed", "Electrical network simulation patch failed and was skipped.");
            }
        }

        public static void AddTransformerDescriptor(Battery battery, List<Descriptor> descriptors)
        {
            if (!TransformerEfficiencyEnabled || battery == null || descriptors == null || battery.powerTransformer == null)
            {
                return;
            }

            var inputPer100J = TransformerEfficiencyCalculator.CalculateInputJoules(100f, TransformerEfficiency);
            var text = "Hardcore Systems: transformer efficiency " + (TransformerEfficiency * 100f).ToString("0.#") + "%";
            var tooltip = "For every 100 J delivered to the output circuit, the input battery consumes about " + inputPer100J.ToString("0.#") + " J.";
            descriptors.Add(new Descriptor(text, tooltip, Descriptor.DescriptorType.Information));
        }

        private static bool ProcessWire(CircuitManager manager, Wire wire, float dt)
        {
            var circuit = wire.NetworkID;
            var wattsUsed = manager.GetWattsUsedByCircuit(circuit);
            var maxSafeWatts = manager.GetMaxSafeWattageForCircuit(circuit);
            if (wattsUsed <= 0f || maxSafeWatts <= 0f)
            {
                return false;
            }

            var circuitLossWatts = 0f;
            if (ElectricalLossesEnabled)
            {
                circuitLossWatts = ElectricalLossCalculator.CalculateCircuitLossWatts(wattsUsed, maxSafeWatts, WireResistanceMultiplier);
                if (circuitLossWatts > 0f && DrainedCircuits.Add(circuit))
                {
                    DrainCircuitBatteries(manager, circuit, circuitLossWatts * dt);
                }
            }

            var heatWatts = 0f;
            if (ElectricalLossesEnabled)
            {
                heatWatts += ElectricalLossCalculator.CalculateWireDissipationWatts(circuitLossWatts);
            }

            if (OverloadHeatEnabled)
            {
                heatWatts += ElectricalLossCalculator.CalculateOverloadHeatWatts(wattsUsed, maxSafeWatts, WireResistanceMultiplier);
            }

            if (heatWatts > 0f)
            {
                ApplyWireHeat(wire, heatWatts, dt);
                return true;
            }

            return circuitLossWatts > 0f;
        }

        private static void DrainCircuitBatteries(CircuitManager manager, ushort circuit, float lossJoules)
        {
            if (lossJoules <= 0f)
            {
                return;
            }

            var batteries = manager.GetBatteriesOnCircuit(circuit);
            if (batteries == null || batteries.Count == 0)
            {
                return;
            }

            var perBattery = lossJoules / batteries.Count;
            for (var i = 0; i < batteries.Count; i++)
            {
                var battery = batteries[i];
                if (battery != null)
                {
                    battery.ConsumeEnergy(perBattery);
                }
            }
        }

        private static void ApplyWireHeat(Wire wire, float heatWatts, float dt)
        {
            var primary = wire.GetComponent<PrimaryElement>();
            if (primary == null || primary.Element == null)
            {
                return;
            }

            var delta = ElectricalLossCalculator.CalculateTemperatureDelta(
                heatWatts,
                dt,
                primary.Mass,
                primary.Element.specificHeatCapacity,
                MaxWireDeltaKelvinPerTick);
            if (delta <= 0f)
            {
                return;
            }

            primary.Temperature += delta;
        }
    }
}
