using System;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public abstract class ComponentIdConditionBase : ICondition
    {
        public TaskId componentId;

        public TaskId UsedComponentId => componentId;

        protected TaskRuntime taskContext;

        public abstract void Bind(TaskRuntime ctx);
        public abstract void Unbind(TaskRuntime ctx);
        public abstract bool IsMet(TaskRuntime ctx);

        protected bool IsTargetMissingOrInactive(TaskRuntime ctx)
        {
            var t = ctx.ResolveTransform(componentId);
            return t == null || !t.gameObject.activeSelf;
        }
    }
}
