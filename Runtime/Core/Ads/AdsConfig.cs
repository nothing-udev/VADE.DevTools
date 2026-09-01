using UnityEngine;

namespace VADE.DevTools.Ads
{
    [CreateAssetMenu(fileName = "AdsConfig", menuName = "Configs/VADE/Ads/AdsConfig")]
    public class AdsConfig : ScriptableObject
    {
        public string androidAppKey;
        public string iosAppKey;

        public string androidBannerAdUnitId;
        public string iosBannerAdUnitId;

        public string androidInterstitialAdUnitId;
        public string iosInterstitialAdUnitId;

        public string androidRewardedAdUnitId;
        public string iosRewardedAdUnitId;

#if UNITY_ANDROID
        public string AppKey => androidAppKey;
        public string BannerAdUnitId => androidBannerAdUnitId;
        public string InterstitialAdUnitId => androidInterstitialAdUnitId;
        public string RewardedAdUnitId => androidRewardedAdUnitId;
#elif UNITY_IOS
        public string AppKey => iosAppKey;
        public string BannerAdUnitId => iosBannerAdUnitId;
        public string InterstitialAdUnitId => iosInterstitialAdUnitId;
        public string RewardedAdUnitId => iosRewardedAdUnitId;
#else
        public string AppKey => string.Empty;
        public string BannerAdUnitId => string.Empty;
        public string InterstitialAdUnitId => string.Empty;
        public string RewardedAdUnitId => string.Empty;
#endif
    }
}
