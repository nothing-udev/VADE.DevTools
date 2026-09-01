using System;

namespace VADE.DevTools.Ads
{
    public interface IAdsService : IDisposable
    {
        void Init();

        bool IsReadyBanner();
        bool ShowBanner();
        bool HideBanner();

        bool IsReadyInterstitial();
        bool ShowInterstitial();

        bool IsReadyRewarded();
        bool ShowRewarded(Action onRewarded = null);
    }
}
