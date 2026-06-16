namespace HardcoreSystems.Modules.ElectricalOverloadThermalDamage
{
    public sealed class OverloadDamageEvent
    {
        public OverloadDamageEvent(string prefabId, string category, string materialId, float currentTemperatureKelvin, float meltingTemperatureKelvin, double injectedEnergyDtu)
        {
            PrefabId = prefabId;
            Category = category;
            MaterialId = materialId;
            CurrentTemperatureKelvin = currentTemperatureKelvin;
            MeltingTemperatureKelvin = meltingTemperatureKelvin;
            InjectedEnergyDtu = injectedEnergyDtu;
        }

        public string PrefabId { get; private set; }
        public string Category { get; private set; }
        public string MaterialId { get; private set; }
        public float CurrentTemperatureKelvin { get; private set; }
        public float MeltingTemperatureKelvin { get; private set; }
        public double InjectedEnergyDtu { get; private set; }
    }
}
