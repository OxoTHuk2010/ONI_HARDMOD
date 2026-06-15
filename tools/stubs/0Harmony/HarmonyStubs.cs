using System;
using System.Reflection;

namespace HarmonyLib
{
    public sealed class Harmony
    {
        public Harmony()
        {
        }

        public MethodInfo Patch(MethodBase original, HarmonyMethod prefix, HarmonyMethod postfix)
        {
            return original as MethodInfo;
        }
    }

    public sealed class HarmonyMethod : Attribute
    {
        public HarmonyMethod()
        {
        }

        public HarmonyMethod(MethodInfo method)
        {
            methodInfo = method;
        }

        public MethodInfo methodInfo;
    }
}
