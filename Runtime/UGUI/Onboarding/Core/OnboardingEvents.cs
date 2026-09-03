namespace VADE.DevTools.Onboarding
{
    public readonly struct StepCompletedEvent
    {
        public readonly int stepIndex;
        public readonly StepDefinition step;

        public StepCompletedEvent(int stepIndex, StepDefinition step)
        {
            this.stepIndex = stepIndex;
            this.step = step;
        }
    }

    public readonly struct OnboardingCompletedEvent { }
}
