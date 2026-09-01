using System;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public abstract class ComponentIdActionsBase : IAction
    {
        public TaskId componentId;

        public TaskId UsedComponentId => componentId;

        public abstract void Enter(TaskRuntime ctx);
        public abstract void Exit(TaskRuntime ctx);
    }
}
