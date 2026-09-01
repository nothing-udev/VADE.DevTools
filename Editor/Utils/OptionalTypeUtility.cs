using System;

namespace VADE.DevTools.Editor.Utils
{

    internal static class OptionalTypeUtility
    {
        public static Type FindType(string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullTypeName);
                if (type != null) return type;
            }
            return null;
        }
    }
}
