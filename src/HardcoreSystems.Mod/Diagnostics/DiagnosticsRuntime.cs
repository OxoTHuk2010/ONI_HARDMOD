using System;
using System.Collections.Generic;
using System.Diagnostics;
using HardcoreSystems.Configuration;

namespace HardcoreSystems.Diagnostics
{
    public static class DiagnosticsRuntime
    {
        private static readonly Dictionary<string, ModuleMetrics> MetricsByModule = new Dictionary<string, ModuleMetrics>();
        private static ModLogger logger;
        private static bool enabled;
        private static int reportIntervalMilliseconds;
        private static int lastReportTick;

        public static void Configure(ModContext context)
        {
            var settings = context.Settings.Diagnostics ?? new DiagnosticsSettings();
            logger = context.Logger;
            enabled = settings.Enabled;
            reportIntervalMilliseconds = (int)Math.Max(1000, settings.ReportIntervalSeconds * 1000f);
            lastReportTick = Environment.TickCount;
            MetricsByModule.Clear();
        }

        public static long Begin()
        {
            return enabled ? Stopwatch.GetTimestamp() : 0L;
        }

        public static void Record(string module, long startTimestamp, int processed, int skipped, int errors)
        {
            if (!enabled)
            {
                return;
            }

            var elapsed = Stopwatch.GetTimestamp() - startTimestamp;
            ModuleMetrics metrics;
            if (!MetricsByModule.TryGetValue(module, out metrics))
            {
                metrics = new ModuleMetrics();
                MetricsByModule[module] = metrics;
            }

            metrics.Add(elapsed, processed, skipped, errors);
            MaybeReport();
        }

        private static void MaybeReport()
        {
            var now = Environment.TickCount;
            if (unchecked(now - lastReportTick) < reportIntervalMilliseconds)
            {
                return;
            }

            lastReportTick = now;
            foreach (var item in MetricsByModule)
            {
                var metrics = item.Value;
                logger.Info(
                    "diagnostics_module_metrics",
                    "Module performance metrics.",
                    "module", item.Key,
                    "calls", metrics.Calls.ToString(),
                    "avgMs", metrics.AverageMilliseconds(),
                    "maxMs", metrics.MaxMilliseconds(),
                    "processed", metrics.Processed.ToString(),
                    "skipped", metrics.Skipped.ToString(),
                    "errors", metrics.Errors.ToString());
            }
        }
    }
}
