namespace HardcoreSystems.Modules.PowerGeneration
{
    public sealed class PowerGenerationProfile
    {
        public PowerGenerationProfile(string prefabId, float wattage, float bodyHeatKilowatts, bool accountsFuelTemperature)
        {
            PrefabId = prefabId;
            Wattage = wattage;
            BodyHeatKilowatts = bodyHeatKilowatts;
            AccountsFuelTemperature = accountsFuelTemperature;
        }

        public string PrefabId { get; private set; }
        public float Wattage { get; private set; }
        public float BodyHeatKilowatts { get; private set; }
        public bool AccountsFuelTemperature { get; private set; }
    }
}
