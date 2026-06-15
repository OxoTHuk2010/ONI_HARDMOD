using UnityEngine;

namespace HardcoreSystems.Bootstrap
{
    public sealed class GameCompatibility
    {
        public GameCompatibility(string unityVersion, string platform)
        {
            UnityVersion = unityVersion;
            Platform = platform;
        }

        public string UnityVersion { get; private set; }
        public string Platform { get; private set; }

        public static GameCompatibility Capture()
        {
            return new GameCompatibility(Application.unityVersion, Application.platform.ToString());
        }
    }
}
