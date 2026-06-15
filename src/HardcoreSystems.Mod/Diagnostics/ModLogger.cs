using System;
using System.Text;
using UnityEngine;

namespace HardcoreSystems.Diagnostics
{
    public sealed class ModLogger
    {
        private readonly string source;
        private readonly RateLimitedLogger rateLimiter = new RateLimitedLogger(TimeSpan.FromSeconds(60));

        public ModLogger(string source, LogLevel minimumLevel)
        {
            this.source = source;
            MinimumLevel = minimumLevel;
        }

        public LogLevel MinimumLevel { get; private set; }

        public void Error(string eventName, string message, params string[] fields)
        {
            Write(LogLevel.Error, eventName, message, fields);
        }

        public void Warning(string eventName, string message, params string[] fields)
        {
            Write(LogLevel.Warning, eventName, message, fields);
        }

        public void Info(string eventName, string message, params string[] fields)
        {
            Write(LogLevel.Info, eventName, message, fields);
        }

        public void Debug(string eventName, string message, params string[] fields)
        {
            Write(LogLevel.Debug, eventName, message, fields);
        }

        public void Trace(string eventName, string message, params string[] fields)
        {
            Write(LogLevel.Trace, eventName, message, fields);
        }

        public void RateLimitedWarning(string key, string eventName, string message, params string[] fields)
        {
            if (rateLimiter.ShouldWrite(key))
            {
                Warning(eventName, message, fields);
            }
        }

        private void Write(LogLevel level, string eventName, string message, string[] fields)
        {
            if (level > MinimumLevel)
            {
                return;
            }

            var line = Format(level, eventName, message, fields);
            if (level == LogLevel.Error)
            {
                DebugLogError(line);
            }
            else if (level == LogLevel.Warning)
            {
                UnityEngine.Debug.LogWarning(line);
            }
            else
            {
                UnityEngine.Debug.Log(line);
            }
        }

        private static void DebugLogError(string line)
        {
            UnityEngine.Debug.LogError(line);
        }

        private string Format(LogLevel level, string eventName, string message, string[] fields)
        {
            var builder = new StringBuilder();
            builder.Append('[').Append(source).Append("] ");
            builder.Append("level=").Append(level);
            builder.Append(" event=").Append(eventName);
            builder.Append(" message=\"").Append(message.Replace("\"", "'")).Append('"');
            for (var i = 0; fields != null && i + 1 < fields.Length; i += 2)
            {
                builder.Append(' ').Append(fields[i]).Append("=\"").Append((fields[i + 1] ?? string.Empty).Replace("\"", "'")).Append('"');
            }

            return builder.ToString();
        }
    }
}
