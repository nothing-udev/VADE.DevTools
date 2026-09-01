using System;

namespace VADE.DevTools.Onboarding
{
    [Flags]
    public enum WaitOptions
    {
        None = 0,
        HandVisibility = 1 << 0,
        WorldArrow = 1 << 1
    }
}
