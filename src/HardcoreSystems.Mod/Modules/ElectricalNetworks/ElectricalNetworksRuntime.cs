using System;
using System.Collections.Generic;
using HardcoreSystems.Diagnostics;
using UnityEngine;

namespace HardcoreSystems.Modules.ElectricalNetworks
{
    internal static class ElectricalNetworksRuntime
    {
        private const int MaxWiresPerTick = 128;
        private const int BrownoutSlots = 10;
        private const int MaxPathSearchCells = 4096;
        private const float MaxLossDeltaKelvinPerTick = 1.50f;
        private const float MaxOverloadDeltaKelvinPerTick = 10f;
        private const float MaxImpulseDeltaKelvin = 50f;

        private static readonly List<Wire> Wires = new List<Wire>();
        private static readonly Dictionary<int, Wire> WireByCell = new Dictionary<int, Wire>();
        private static readonly Queue<int> SearchQueue = new Queue<int>();
        private static readonly Dictionary<int, int> SearchParentByCell = new Dictionary<int, int>();
        private static readonly HashSet<int> SourceCells = new HashSet<int>();
        private static readonly List<Wire> PathWires = new List<Wire>();
        private static ModLogger logger;
        private static int nextWireIndex;
        private static bool topologyDirty = true;

        public static bool ElectricalLossesEnabled { get; private set; }
        public static bool OverloadHeatEnabled { get; private set; }
        public static float WireResistanceMultiplier { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            var settings = context.Settings.Power;
            ElectricalLossesEnabled = settings.ElectricalLossesEnabled && settings.WireResistanceMultiplier > 0.0001f;
            OverloadHeatEnabled = settings.OverloadHeatEnabled;
            WireResistanceMultiplier = settings.WireResistanceMultiplier;
        }

        public static void RegisterWire(Wire wire)
        {
            if (wire == null || Wires.Contains(wire))
            {
                return;
            }

            Wires.Add(wire);
            topologyDirty = true;
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

            topologyDirty = true;
        }

        public static void SimulateCircuitTick(CircuitManager manager, float dt)
        {
            var start = DiagnosticsRuntime.Begin();
            var processed = 0;
            var skipped = 0;

            try
            {
                if (manager == null || dt <= 0f || Wires.Count == 0 || !OverloadHeatEnabled)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                EnsureTopology();
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
                        topologyDirty = true;
                        skipped++;
                        continue;
                    }

                    var circuit = wire.NetworkID;
                    var wattsUsed = manager.GetWattsUsedByCircuit(circuit);
                    var maxSafeWatts = manager.GetMaxSafeWattageForCircuit(circuit);
                    var overloadHeatWatts = ElectricalLossCalculator.CalculateOverloadHeatWatts(wattsUsed, maxSafeWatts, WireResistanceMultiplier);
                    if (overloadHeatWatts <= 0f)
                    {
                        skipped++;
                        continue;
                    }

                    ApplyWireHeat(wire, overloadHeatWatts, dt, MaxOverloadDeltaKelvinPerTick);
                    processed++;
                }

                DiagnosticsRuntime.Record("ElectricalNetworks", start, processed, skipped, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("ElectricalNetworks", start, processed, skipped, 1);
                logger.RateLimitedWarning("electrical_network_tick_failed", "electrical_network_tick_failed", "Electrical network simulation patch failed and was skipped.");
            }
        }

        public static void ApplyConsumerBrownout(EnergyConsumer consumer, float dt)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!ElectricalLossesEnabled || consumer == null || dt <= 0f || !consumer.IsPowered)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                EnsureTopology();
                var requiredWatts = consumer.WattsNeededWhenActive;
                if (requiredWatts <= 0f)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var manager = Game.Instance == null ? null : Game.Instance.circuitManager;
                if (manager == null)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var path = BuildPathToSource(manager, consumer);
                if (path == null || path.Wires.Count == 0)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var lossWatts = ElectricalLossCalculator.CalculateOhmicLossWatts(requiredWatts, path.ResistanceOhms);
                var availability = ElectricalLossCalculator.CalculateAvailability(requiredWatts, lossWatts);
                ApplyPathHeat(path.Wires, lossWatts, dt);

                var phase = (Environment.TickCount / 200) + consumer.GetHashCode();
                if (!ElectricalLossCalculator.ShouldPowerConsumer(availability, phase, BrownoutSlots))
                {
                    SetConsumerPowered(consumer, false);
                }

                DiagnosticsRuntime.Record("ElectricalNetworks", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 0, 1);
                logger.RateLimitedWarning("consumer_brownout_failed", "consumer_brownout_failed", "Electrical brownout patch failed and was skipped.");
            }
        }

        public static void ApplyDamageImpulse(BuildingHP buildingHp, int damage)
        {
            var start = DiagnosticsRuntime.Begin();
            try
            {
                if (!OverloadHeatEnabled || buildingHp == null)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var wire = buildingHp.GetComponent<Wire>();
                if (wire == null)
                {
                    DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 1, 0);
                    return;
                }

                var impulseWatts = ElectricalLossCalculator.CalculateShortCircuitImpulseWatts(damage);
                ApplyWireHeat(wire, impulseWatts, 1f, MaxImpulseDeltaKelvin);
                ApplyNeighborImpulse(wire, impulseWatts * 0.35f);
                DiagnosticsRuntime.Record("ElectricalNetworks", start, 1, 0, 0);
            }
            catch (Exception)
            {
                DiagnosticsRuntime.Record("ElectricalNetworks", start, 0, 0, 1);
                logger.RateLimitedWarning("wire_damage_impulse_failed", "wire_damage_impulse_failed", "Wire damage impulse failed and was skipped.");
            }
        }

        private static void EnsureTopology()
        {
            if (!topologyDirty)
            {
                return;
            }

            WireByCell.Clear();
            for (var i = Wires.Count - 1; i >= 0; i--)
            {
                var wire = Wires[i];
                if (wire == null)
                {
                    Wires.RemoveAt(i);
                    continue;
                }

                var cell = Grid.PosToCell(wire);
                if (Grid.IsValidCell(cell))
                {
                    WireByCell[cell] = wire;
                }
            }

            topologyDirty = false;
        }

        private static PowerPath BuildPathToSource(CircuitManager manager, EnergyConsumer consumer)
        {
            var startCell = consumer.PowerCell;
            if (!Grid.IsValidCell(startCell) || !WireByCell.ContainsKey(startCell))
            {
                return null;
            }

            SourceCells.Clear();
            AddSourceCells(manager.GetGeneratorsOnCircuit(consumer.CircuitID), consumer.CircuitID);
            AddSourceCells(manager.GetBatteriesOnCircuit(consumer.CircuitID), consumer.CircuitID);
            if (SourceCells.Count == 0)
            {
                return null;
            }

            SearchQueue.Clear();
            SearchParentByCell.Clear();
            SearchQueue.Enqueue(startCell);
            SearchParentByCell[startCell] = -1;

            var foundCell = -1;
            while (SearchQueue.Count > 0 && SearchParentByCell.Count < MaxPathSearchCells)
            {
                var cell = SearchQueue.Dequeue();
                if (SourceCells.Contains(cell))
                {
                    foundCell = cell;
                    break;
                }

                EnqueueNeighbor(cell, Grid.CellAbove(cell));
                EnqueueNeighbor(cell, Grid.CellBelow(cell));
                EnqueueNeighbor(cell, Grid.CellLeft(cell));
                EnqueueNeighbor(cell, Grid.CellRight(cell));
            }

            if (foundCell < 0)
            {
                return null;
            }

            PathWires.Clear();
            var resistance = 0f;
            var pathCell = foundCell;
            while (pathCell >= 0)
            {
                Wire wire;
                if (WireByCell.TryGetValue(pathCell, out wire) && wire != null)
                {
                    PathWires.Add(wire);
                    resistance += CalculateWireResistance(wire);
                }

                int parent;
                if (!SearchParentByCell.TryGetValue(pathCell, out parent))
                {
                    break;
                }

                pathCell = parent;
            }

            if (PathWires.Count == 0 || resistance <= 0f)
            {
                return null;
            }

            return new PowerPath(new List<Wire>(PathWires), resistance);
        }

        private static void EnqueueNeighbor(int parent, int neighbor)
        {
            if (!Grid.IsValidCell(neighbor) || SearchParentByCell.ContainsKey(neighbor) || !WireByCell.ContainsKey(neighbor))
            {
                return;
            }

            SearchParentByCell[neighbor] = parent;
            SearchQueue.Enqueue(neighbor);
        }

        private static void AddSourceCells(List<Generator> generators, ushort circuit)
        {
            if (generators == null)
            {
                return;
            }

            for (var i = 0; i < generators.Count; i++)
            {
                var generator = generators[i];
                if (generator == null || generator.CircuitID != circuit || generator.WattageRating <= 0f)
                {
                    continue;
                }

                var cell = generator.PowerCell;
                if (Grid.IsValidCell(cell))
                {
                    SourceCells.Add(cell);
                }
            }
        }

        private static void AddSourceCells(List<Battery> batteries, ushort circuit)
        {
            if (batteries == null)
            {
                return;
            }

            for (var i = 0; i < batteries.Count; i++)
            {
                var battery = batteries[i];
                if (battery == null || battery.CircuitID != circuit || battery.JoulesAvailable <= 0f || battery.powerTransformer != null)
                {
                    continue;
                }

                var cell = battery.PowerCell;
                if (Grid.IsValidCell(cell))
                {
                    SourceCells.Add(cell);
                }
            }
        }

        private static float CalculateWireResistance(Wire wire)
        {
            var primary = wire.GetComponent<PrimaryElement>();
            var temperature = primary == null ? 293.15f : primary.Temperature;
            var materialFactor = GetMaterialResistanceFactor(primary);
            var capacityWatts = Wire.GetMaxWattageAsFloat(wire.MaxWattageRating);
            return ElectricalLossCalculator.CalculateCellResistanceOhms(
                WireResistanceMultiplier,
                materialFactor,
                temperature,
                capacityWatts);
        }

        private static float GetMaterialResistanceFactor(PrimaryElement primary)
        {
            if (primary == null)
            {
                return 1f;
            }

            var name = primary.ElementID.ToString();
            if (name.IndexOf("Aluminum", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0.45f;
            }

            if (name.IndexOf("Gold", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0.60f;
            }

            if (name.IndexOf("Copper", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0.75f;
            }

            if (name.IndexOf("Steel", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0.85f;
            }

            if (name.IndexOf("Lead", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 1.40f;
            }

            return 1f;
        }

        private static void ApplyPathHeat(List<Wire> path, float lossWatts, float dt)
        {
            if (lossWatts <= 0f || path == null || path.Count == 0)
            {
                return;
            }

            var wattsPerWire = lossWatts / path.Count;
            for (var i = 0; i < path.Count; i++)
            {
                ApplyWireHeat(path[i], wattsPerWire, dt, MaxLossDeltaKelvinPerTick);
            }
        }

        private static void ApplyNeighborImpulse(Wire source, float impulseWatts)
        {
            EnsureTopology();
            var sourceCell = Grid.PosToCell(source);
            ApplyNeighborWireHeat(Grid.CellAbove(sourceCell), impulseWatts);
            ApplyNeighborWireHeat(Grid.CellBelow(sourceCell), impulseWatts);
            ApplyNeighborWireHeat(Grid.CellLeft(sourceCell), impulseWatts);
            ApplyNeighborWireHeat(Grid.CellRight(sourceCell), impulseWatts);
        }

        private static void ApplyNeighborWireHeat(int cell, float impulseWatts)
        {
            Wire wire;
            if (Grid.IsValidCell(cell) && WireByCell.TryGetValue(cell, out wire))
            {
                ApplyWireHeat(wire, impulseWatts, 1f, MaxImpulseDeltaKelvin * 0.35f);
            }
        }

        private static void ApplyWireHeat(Wire wire, float heatWatts, float dt, float maxDeltaKelvin)
        {
            if (wire == null)
            {
                return;
            }

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
                maxDeltaKelvin);
            if (delta <= 0f)
            {
                return;
            }

            primary.Temperature += delta;
        }

        private static void SetConsumerPowered(EnergyConsumer consumer, bool powered)
        {
            var operational = consumer.GetComponent<Operational>();
            if (operational != null)
            {
                operational.SetFlag(EnergyConsumer.PoweredFlag, powered);
            }
        }

        private sealed class PowerPath
        {
            public PowerPath(List<Wire> wires, float resistanceOhms)
            {
                Wires = wires;
                ResistanceOhms = resistanceOhms;
            }

            public List<Wire> Wires { get; private set; }

            public float ResistanceOhms { get; private set; }
        }
    }
}
