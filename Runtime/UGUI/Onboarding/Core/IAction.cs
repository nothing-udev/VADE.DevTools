namespace VADE.DevTools.Onboarding
{
    public interface IAction : IUsesComponent
    {
        void Enter(TaskRuntime ctx);
        void Exit(TaskRuntime ctx);
    }
}
