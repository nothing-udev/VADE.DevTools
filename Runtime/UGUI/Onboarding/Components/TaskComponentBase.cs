using VADE.DevTools.Attributes;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Onboarding
{
    public abstract class TaskComponentBase : ReactiveBehaviour
    {
        [GeneratedId] public string id;

        public TaskId Id => new(id);

        protected virtual void Start()
        {
            if (string.IsNullOrEmpty(id))
                id = System.Guid.NewGuid().ToString("N");

            if (OnboardingService.Instance != null)
                OnboardingService.Instance.Register(this);
        }

        protected override void OnDestroy()
        {
            if (OnboardingService.Instance != null)
                OnboardingService.Instance.Unregister(this);

            base.OnDestroy();
        }

        protected void Complete() => TaskEvents.RaiseCompleted(Id);
        protected void Report(float p) => TaskEvents.RaiseProgress(Id, p);
    }
}
