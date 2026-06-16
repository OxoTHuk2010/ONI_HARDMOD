using UnityEngine;

namespace HardcoreSystems.Modules.Thermal
{
    internal static class BuildingThermalAdapter
    {
        private const double DtuPerKilodtu = 1000.0;

        public static bool TryAddEnergy(GameObject gameObject, double energyDtu, float minTemperatureKelvin, float maxTemperatureKelvin)
        {
            if (gameObject == null
                || double.IsNaN(energyDtu)
                || double.IsInfinity(energyDtu)
                || energyDtu <= 0.0
                || maxTemperatureKelvin <= 0f)
            {
                return false;
            }

            var handle = GameComps.StructureTemperatures.GetHandle(gameObject);
            if (!GameComps.StructureTemperatures.IsValid(handle))
            {
                return false;
            }

            var payload = GameComps.StructureTemperatures.GetPayload(handle);
            if (payload.simHandleCopy < 0)
            {
                return false;
            }

            var deltaKilodtu = (float)(energyDtu / DtuPerKilodtu);
            SimMessages.ModifyBuildingEnergy(payload.simHandleCopy, deltaKilodtu, minTemperatureKelvin, maxTemperatureKelvin);
            return true;
        }
    }
}
