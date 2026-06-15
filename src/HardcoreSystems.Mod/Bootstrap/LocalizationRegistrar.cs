namespace HardcoreSystems.Bootstrap
{
    public static class LocalizationRegistrar
    {
        public static void Register(ModContext context)
        {
            context.Logger.Info("localization_ready", "Localization placeholders are available.", "fallback", "en");
        }
    }
}
