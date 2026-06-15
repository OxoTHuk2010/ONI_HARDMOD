using System;
using System.Reflection;
using HarmonyLib;

namespace HardcoreSystems.Bootstrap
{
    public sealed class PatchResult
    {
        private PatchResult(bool success, string moduleId, string target, string error)
        {
            Success = success;
            ModuleId = moduleId;
            Target = target;
            Error = error;
        }

        public bool Success { get; private set; }
        public string ModuleId { get; private set; }
        public string Target { get; private set; }
        public string Error { get; private set; }

        public static PatchResult Ok(string moduleId, MethodBase target)
        {
            return new PatchResult(true, moduleId, Describe(target), null);
        }

        public static PatchResult Failed(string moduleId, MethodBase target, string error)
        {
            return new PatchResult(false, moduleId, Describe(target), error);
        }

        private static string Describe(MethodBase target)
        {
            return target == null ? "<null>" : target.DeclaringType.FullName + "." + target.Name;
        }
    }

    public static class PatchGuard
    {
        public static PatchResult TryPatch(
            Harmony harmony,
            MethodBase target,
            HarmonyMethod prefix,
            HarmonyMethod postfix,
            string moduleId)
        {
            if (target == null)
            {
                return PatchResult.Failed(moduleId, null, "Target method was not found.");
            }

            try
            {
                harmony.Patch(target, prefix, postfix);
                return PatchResult.Ok(moduleId, target);
            }
            catch (Exception ex)
            {
                return PatchResult.Failed(moduleId, target, ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}
