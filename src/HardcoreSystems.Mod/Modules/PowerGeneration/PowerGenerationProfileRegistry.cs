using System.Collections.Generic;

namespace HardcoreSystems.Modules.PowerGeneration
{
    public static class PowerGenerationProfileRegistry
    {
        private static readonly Dictionary<string, PowerGenerationProfile> Profiles =
            new Dictionary<string, PowerGenerationProfile>
            {
                { "ManualGenerator", new PowerGenerationProfile("ManualGenerator", 400f, 1f, false) },
                { "WoodGasGenerator", new PowerGenerationProfile("WoodGasGenerator", 300f, 20f, true) },
                { "Generator", new PowerGenerationProfile("Generator", 600f, 40f, true) },
                { "PeatGenerator", new PowerGenerationProfile("PeatGenerator", 480f, 22.5f, true) },
                { "HydrogenGenerator", new PowerGenerationProfile("HydrogenGenerator", 800f, 32f, true) },
                { "MethaneGenerator", new PowerGenerationProfile("MethaneGenerator", 800f, 20f, true) },
                { "PetroleumGenerator", new PowerGenerationProfile("PetroleumGenerator", 2000f, 50f, true) },
            };

        public static bool TryGet(string prefabId, out PowerGenerationProfile profile)
        {
            if (string.IsNullOrEmpty(prefabId))
            {
                profile = null;
                return false;
            }

            return Profiles.TryGetValue(prefabId, out profile);
        }

        public static bool IsProfiledGenerator(BuildingDef def)
        {
            PowerGenerationProfile profile;
            return def != null && TryGet(def.PrefabID, out profile);
        }
    }
}
