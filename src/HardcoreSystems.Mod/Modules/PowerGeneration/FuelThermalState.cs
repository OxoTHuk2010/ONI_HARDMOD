using System.Collections.Generic;

namespace HardcoreSystems.Modules.PowerGeneration
{
    internal sealed class FuelThermalState
    {
        public FuelThermalState()
        {
            Samples = new List<FuelThermalSample>();
        }

        public List<FuelThermalSample> Samples { get; private set; }

        public bool HasSamples
        {
            get { return Samples.Count > 0; }
        }
    }

    internal sealed class FuelThermalSample
    {
        public FuelThermalSample(Tag tag, float massBeforeKilograms, float temperatureKelvin, float specificHeatCapacity)
        {
            Tag = tag;
            MassBeforeKilograms = massBeforeKilograms;
            TemperatureKelvin = temperatureKelvin;
            SpecificHeatCapacity = specificHeatCapacity;
        }

        public Tag Tag { get; private set; }
        public float MassBeforeKilograms { get; private set; }
        public float TemperatureKelvin { get; private set; }
        public float SpecificHeatCapacity { get; private set; }
    }
}
