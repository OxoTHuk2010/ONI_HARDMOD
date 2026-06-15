using System;
using System.Collections.Generic;

namespace HardcoreSystems.Diagnostics
{
    public sealed class RateLimitedLogger
    {
        private readonly int intervalMilliseconds;
        private readonly Dictionary<string, int> lastWriteTick = new Dictionary<string, int>();

        public RateLimitedLogger(TimeSpan interval)
        {
            intervalMilliseconds = (int)Math.Min(int.MaxValue, Math.Max(0, interval.TotalMilliseconds));
        }

        public bool ShouldWrite(string key)
        {
            var now = Environment.TickCount;
            int last;
            if (lastWriteTick.TryGetValue(key, out last) && unchecked(now - last) < intervalMilliseconds)
            {
                return false;
            }

            lastWriteTick[key] = now;
            return true;
        }
    }
}
