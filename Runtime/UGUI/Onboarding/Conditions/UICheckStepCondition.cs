using UnityEngine;

namespace VADE.DevTools.Onboarding
{
    public class UICheckStepCondition : MonoBehaviour
    {
        [SerializeField] private string requiredStepId;
        [SerializeField] private GameObject tutorialHand;

        private void OnEnable()
        {
            if (OnboardingService.Instance != null && OnboardingService.Instance.CurrentStepUID == requiredStepId)
                tutorialHand.SetActive(true);
        }

        private void OnDisable() => tutorialHand.SetActive(false);
    }
}
