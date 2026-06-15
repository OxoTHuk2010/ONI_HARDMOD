using HardcoreSystems.Bootstrap;
using HardcoreSystems.Diagnostics;

namespace HardcoreSystems.Modules
{
    internal static class ModulePatchReporter
    {
        public static void Log(ModLogger logger, PatchResult result)
        {
            if (result.Success)
            {
                logger.Info("module_patch_registered", "Module patch registered.", "module", result.ModuleId, "target", result.Target);
            }
            else
            {
                logger.Warning("module_patch_failed", "Module patch was not registered.", "module", result.ModuleId, "target", result.Target, "error", result.Error);
            }
        }
    }
}
