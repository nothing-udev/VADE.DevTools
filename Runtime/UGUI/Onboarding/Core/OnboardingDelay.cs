using System;
using System.Collections;
using UnityEngine;
using VADE.DevTools.Utilities;
#if VADE_DOTWEEN
using DG.Tweening;
#endif

namespace VADE.DevTools.Onboarding
{
    internal static class OnboardingDelay
    {
        private static CoroutineRunner runner;

        public static void Call(float seconds, Action action)
        {
#if VADE_DOTWEEN
            DOVirtual.DelayedCall(seconds, () => action());
#else
            if (runner == null)
            {
                var go = new GameObject("[OnboardingDelay]");
                UnityEngine.Object.DontDestroyOnLoad(go);
                runner = go.AddComponent<CoroutineRunner>();
            }
            runner.Run(Wait(seconds, action));
#endif
        }

#if !VADE_DOTWEEN
        private static IEnumerator Wait(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }
#endif
    }
}
