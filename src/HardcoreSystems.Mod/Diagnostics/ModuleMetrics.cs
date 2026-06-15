using System.Diagnostics;

namespace HardcoreSystems.Diagnostics
{
    public sealed class ModuleMetrics
    {
        public long Calls { get; private set; }
        public long TotalTicks { get; private set; }
        public long MaxTicks { get; private set; }
        public long Processed { get; private set; }
        public long Skipped { get; private set; }
        public long Errors { get; private set; }

        public void Add(long elapsedTicks, int processed, int skipped, int errors)
        {
            Calls++;
            TotalTicks += elapsedTicks;
            if (elapsedTicks > MaxTicks)
            {
                MaxTicks = elapsedTicks;
            }

            Processed += processed;
            Skipped += skipped;
            Errors += errors;
        }

        public string AverageMilliseconds()
        {
            if (Calls == 0)
            {
                return "0";
            }

            return TicksToMilliseconds(TotalTicks / Calls);
        }

        public string MaxMilliseconds()
        {
            return TicksToMilliseconds(MaxTicks);
        }

        private static string TicksToMilliseconds(long ticks)
        {
            var ms = ticks * 1000.0 / Stopwatch.Frequency;
            return ms.ToString("0.###");
        }
    }
}
