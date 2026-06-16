using System.Collections.Generic;
using UnityEngine;

namespace HardcoreSystems.Modules.ElectricalOverloadThermalDamage
{
    internal sealed class OverloadEventDeduplicator
    {
        private readonly Dictionary<int, int> lastFrameByInstance = new Dictionary<int, int>();

        public bool TryEnter(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            var instanceId = gameObject.GetInstanceID();
            var frame = Time.frameCount;
            int lastFrame;
            if (lastFrameByInstance.TryGetValue(instanceId, out lastFrame) && lastFrame == frame)
            {
                return false;
            }

            lastFrameByInstance[instanceId] = frame;
            return true;
        }
    }
}
