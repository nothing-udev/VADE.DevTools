using System.Collections.Generic;
using UnityEngine;

namespace VADE.DevTools.Onboarding
{
    [CreateAssetMenu(menuName = "Configs/VADE/Onboarding/Asset", fileName = "Onboarding_Asset")]
    public class OnboardingAsset : ScriptableObject
    {
        public List<TaskDefinition> tasks = new();
    }
}
