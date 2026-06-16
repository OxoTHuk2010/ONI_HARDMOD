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

        public double OutputEnergyAssignedDtu { get; private set; }

        public float MaximumTemperatureKelvin
        {
            get
            {
                var maximum = 0f;
                for (var i = 0; i < Samples.Count; i++)
                {
                    if (Samples[i].TemperatureKelvin > maximum)
                    {
                        maximum = Samples[i].TemperatureKelvin;
                    }
                }

                return maximum;
            }
        }

        public void AddOutputEnergyAssigned(double energyDtu)
        {
            if (energyDtu > 0.0)
            {
                OutputEnergyAssignedDtu += energyDtu;
            }
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
