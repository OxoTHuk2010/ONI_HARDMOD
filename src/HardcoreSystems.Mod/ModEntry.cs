using HarmonyLib;
using HardcoreSystems.Bootstrap;

namespace HardcoreSystems
{
    public sealed class ModEntry : KMod.UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            ModBootstrap.Initialize(this, harmony);
        }
    }
}
