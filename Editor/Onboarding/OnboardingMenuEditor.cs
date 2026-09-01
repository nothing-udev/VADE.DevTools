using UnityEditor;
using UnityEngine;
using VADE.DevTools.Onboarding;

namespace VADE.DevTools.Editor.Onboarding
{
    public static class OnboardingMenuEditor
    {
        [MenuItem("Tools/VADE/Onboarding/Create Asset")]
        private static void CreateOnboarding()
        {
            var asset = ScriptableObject.CreateInstance<OnboardingAsset>();
            ProjectWindowUtil.CreateAsset(asset, "Onboarding_Asset.asset");
        }

        [MenuItem("Tools/VADE/Onboarding/Clear Saves")]
        private static void ClearOnboarding() => OnboardingSave.Delete();
    }
}
