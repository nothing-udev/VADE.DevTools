#if VADE_LEVELPLAY
using System;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace VADE.DevTools.Ads
{
    public class AdsServiceLevelPlay : IAdsService
    {
        private readonly AdsConfig config;
        public Func<bool> IsBlocked { get; set; } = () => false;

        private LevelPlayBannerAd bannerAd;
        private LevelPlayInterstitialAd interstitialAd;
        private LevelPlayRewardedAd rewardedAd;

        private bool adsEnabled;
        private Action onRewardedCallback;

        public AdsServiceLevelPlay(AdsConfig config)
        {
            this.config = config;
        }

        public void Init()
        {
            LevelPlay.ValidateIntegration();
            LevelPlay.OnInitSuccess += OnInitSuccess;
            LevelPlay.OnInitFailed += OnInitFailed;
            LevelPlay.Init(config.AppKey);
        }

        private void OnInitSuccess(LevelPlayConfiguration configuration) => EnableAds();

        private void OnInitFailed(LevelPlayInitError error) =>
            Debug.LogWarning("[AdsServiceLevelPlay] Init failed: " + error);

        private void EnableAds()
        {
            rewardedAd = new LevelPlayRewardedAd(config.RewardedAdUnitId);
            rewardedAd.OnAdRewarded += OnAdRewarded;
            rewardedAd.LoadAd();

            bannerAd = new LevelPlayBannerAd(config.BannerAdUnitId);
            bannerAd.LoadAd();

            interstitialAd = new LevelPlayInterstitialAd(config.InterstitialAdUnitId);
            interstitialAd.OnAdClosed += OnInterstitialClosed;
            interstitialAd.LoadAd();

            adsEnabled = true;
        }

        private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward) => onRewardedCallback?.Invoke();

        private void OnInterstitialClosed(LevelPlayAdInfo info) => interstitialAd.LoadAd();

        public bool IsReadyBanner()
        {
            if (IsBlocked()) return false;
            return adsEnabled;
        }

        public bool ShowBanner()
        {
            if (!IsReadyBanner()) return false;
            bannerAd?.ShowAd();
            return true;
        }

        public bool HideBanner()
        {
            if (bannerAd == null) return false;
            bannerAd.HideAd();
            return true;
        }

        public bool IsReadyInterstitial() => interstitialAd != null && interstitialAd.IsAdReady();

        public bool ShowInterstitial()
        {
            if (IsBlocked()) return false;

            if (!IsReadyInterstitial())
            {
                interstitialAd?.LoadAd();
                return false;
            }

            interstitialAd.ShowAd();
            return true;
        }

        public bool IsReadyRewarded() => rewardedAd != null && rewardedAd.IsAdReady();

        public bool ShowRewarded(Action onRewarded = null)
        {
            if (!IsReadyRewarded())
            {
                rewardedAd?.LoadAd();
                return false;
            }

            onRewardedCallback = onRewarded;
            rewardedAd.ShowAd();
            return true;
        }

        public void Dispose()
        {
            LevelPlay.OnInitSuccess -= OnInitSuccess;
            LevelPlay.OnInitFailed -= OnInitFailed;

            bannerAd?.DestroyAd();
            interstitialAd?.DestroyAd();
        }
    }
}
#endif
