using System;
using UnityEngine.Playables;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public class ShowUiHand : ComponentIdActionsBase
    {
        public HandAnimation animationType = HandAnimation.None;

        public override string ToString() => $"ShowUiHand -> {componentId}";

        public override void Enter(TaskRuntime ctx)
        {
            var t = ctx.ResolveUI(componentId);
            if (t != null) ctx.UiHand.Show(t, animationType);
        }

        public override void Exit(TaskRuntime ctx) => ctx.UiHand.Hide();
    }

    [Serializable]
    public class ShowWorldArrow : ComponentIdActionsBase
    {
        public override void Enter(TaskRuntime ctx)
        {
            var tr = ctx.ResolveTransform(componentId);
            if (tr != null) ctx.WorldArrow.Show(tr);
        }

        public override void Exit(TaskRuntime ctx) => ctx.WorldArrow.Hide();
    }

    [Serializable]
    public class PlayCutscene : ComponentIdActionsBase
    {
        public PlayableDirector director;
        public bool waitForEnd = true;

        private TaskRuntime runtimeCtx;

        public override void Enter(TaskRuntime ctx)
        {
            runtimeCtx = ctx;
            if (director != null) director.Play();

            if (!waitForEnd) ctx.RequestStepComplete();
            else if (director != null) director.stopped += OnStopped;
        }

        private void OnStopped(PlayableDirector d)
        {
            d.stopped -= OnStopped;
            runtimeCtx.RequestStepComplete();
        }

        public override void Exit(TaskRuntime ctx) { }
    }
}
