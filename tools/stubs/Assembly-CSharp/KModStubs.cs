using System.Reflection;
using HarmonyLib;

namespace KMod
{
    public class UserMod2
    {
        public Assembly assembly { get; set; }

        public string path { get; set; }

        public virtual void OnLoad(Harmony harmony)
        {
        }
    }
}
