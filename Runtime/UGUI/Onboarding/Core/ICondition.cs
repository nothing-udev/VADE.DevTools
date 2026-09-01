namespace VADE.DevTools.Onboarding
{
    public interface ICondition : IUsesComponent
    {
        void Bind(TaskRuntime ctx);
        void Unbind(TaskRuntime ctx);
        bool IsMet(TaskRuntime ctx);
    }
}
