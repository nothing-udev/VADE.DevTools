using UnityEngine;

namespace VADE.DevTools.Onboarding
{
    public class TaskRuntime
    {
        public OnboardingService Service { get; internal set; }
        public UiHandPointer UiHand { get; internal set; }
        public WorldArrowPointer WorldArrow { get; internal set; }
        public TaskDefinition CurrentTask { get; internal set; }
        public int CurrentStepIndex { get; internal set; }

        public RectTransform ResolveUI(TaskId id) => Service.ResolveUI(id);
        public Transform ResolveTransform(TaskId id) => Service.ResolveTransform(id);
        public void RequestStepComplete() => Service.RequestStepComplete();
    }
}
