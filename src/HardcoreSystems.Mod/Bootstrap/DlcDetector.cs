using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HardcoreSystems.Bootstrap
{
    public sealed class DlcInfo
    {
        public DlcInfo(IList<string> installedDlcIds)
        {
            InstalledDlcIds = new List<string>(installedDlcIds);
        }

        public IList<string> InstalledDlcIds { get; private set; }

        public bool HasAnyDlc
        {
            get { return InstalledDlcIds.Count > 0; }
        }

        public string ToSummary()
        {
            return InstalledDlcIds.Count == 0 ? "none" : string.Join(",", new List<string>(InstalledDlcIds).ToArray());
        }
    }

    public static class DlcDetector
    {
        public static DlcInfo Detect()
        {
            var ids = new List<string>();
            try
            {
                var dataPath = Application.dataPath;
                var dlcRoot = Path.Combine(dataPath, "StreamingAssets", "dlc");
                if (Directory.Exists(dlcRoot))
                {
                    foreach (var directory in Directory.GetDirectories(dlcRoot))
                    {
                        ids.Add(Path.GetFileName(directory));
                    }
                }
            }
            catch (Exception)
            {
                ids.Add("unknown");
            }

            ids.Sort(StringComparer.OrdinalIgnoreCase);
            return new DlcInfo(ids);
        }
    }
}
