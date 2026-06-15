using HardcoreSystems.Configuration;
using HardcoreSystems.Diagnostics;

namespace HardcoreSystems.Modules.MiningYield
{
    internal static class MiningYieldRuntime
    {
        private static readonly MiningYieldDigTracker digTracker = new MiningYieldDigTracker();
        private static ModLogger logger;
        private static int loggedAdjustments;

        public static bool Enabled { get; private set; }
        public static float YieldMultiplier { get; private set; }

        public static void Configure(ModContext context)
        {
            logger = context.Logger;
            var settings = context.Settings.Mining;
            Enabled = settings.Enabled && settings.YieldMultiplier < 0.9999f;
            YieldMultiplier = settings.YieldMultiplier;
            digTracker.Clear();
            loggedAdjustments = 0;
        }

        public static void ApplyToCompletedDig(int gameCell, ref float mass, ushort elementIndex)
        {
            if (!Enabled)
            {
                return;
            }

            var original = mass;
            mass = MiningYieldCalculator.Apply(mass, YieldMultiplier);
            if (mass < original && loggedAdjustments < 20)
            {
                loggedAdjustments++;
                logger.Info("mining_yield_applied", "Mining yield adjusted completed dig mass.", "cell", gameCell.ToString(), "elementIndex", elementIndex.ToString(), "from", original.ToString("0.###"), "to", mass.ToString("0.###"));
            }
        }
    }
}
