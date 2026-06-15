using System;
using System.Collections.Generic;
using HarmonyLib;
using HardcoreSystems.Diagnostics;
using HardcoreSystems.Modules;

namespace HardcoreSystems
{
    public sealed class ModuleRegistry
    {
        private delegate void ModuleOperation();

        private readonly List<IGameplayModule> modules = new List<IGameplayModule>();
        private readonly ModLogger logger;

        public ModuleRegistry(ModLogger logger)
        {
            this.logger = logger;
        }

        public void Register(IGameplayModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            modules.Add(module);
        }

        public void Initialize(ModContext context)
        {
            foreach (var module in modules)
            {
                Execute(module, "initialize", delegate() { module.Initialize(context); });
            }
        }

        public void RegisterPatches(Harmony harmony, ModContext context)
        {
            foreach (var module in modules)
            {
                if (!module.IsEnabled(context))
                {
                    logger.Debug("module_disabled", "Module is disabled.", "module", module.Id);
                    continue;
                }

                Execute(module, "register_patches", delegate() { module.RegisterPatches(harmony, context); });
            }
        }

        public IList<string> GetModuleIds()
        {
            var ids = new List<string>();
            foreach (var module in modules)
            {
                ids.Add(module.Id);
            }

            return ids;
        }

        private void Execute(IGameplayModule module, string operation, ModuleOperation action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                logger.Error(
                    "module_operation_failed",
                    "Module operation failed.",
                    "module", module.Id,
                    "operation", operation,
                    "exception", ex.GetType().Name,
                    "message", ex.Message);
            }
        }
    }
}
