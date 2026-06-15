namespace UnityEngine
{
    public enum RuntimePlatform
    {
        WindowsPlayer = 2
    }

    public static class Application
    {
        public static string dataPath
        {
            get { return string.Empty; }
        }

        public static string unityVersion
        {
            get { return string.Empty; }
        }

        public static RuntimePlatform platform
        {
            get { return RuntimePlatform.WindowsPlayer; }
        }
    }

    public static class Debug
    {
        public static void Log(object message)
        {
        }

        public static void LogWarning(object message)
        {
        }

        public static void LogError(object message)
        {
        }
    }
}
