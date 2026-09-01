using System.Collections.Generic;

namespace VADE.DevTools.Onboarding
{
    internal static class ListExtensions
    {
        public static bool IsValidIndex<T>(this List<T> list, int index) => list != null && index >= 0 && index < list.Count;
    }
}
