using System;
using UnityEngine;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public class WaitForComponentCompleted : ComponentIdConditionBase
    {
        private bool done;

        public override void Bind(TaskRuntime ctx)
        {
            taskContext = ctx;
            done = false;
            TaskEvents.ComponentCompleted += OnCompleted;
        }

        public override void Unbind(TaskRuntime ctx) => TaskEvents.ComponentCompleted -= OnCompleted;

        private void OnCompleted(TaskId id)
        {
            if (!id.Equals(componentId)) return;
            done = true;
            taskContext.Service.RequestStepComplete();
        }

        public override bool IsMet(TaskRuntime ctx) => done;
    }

    [Serializable]
    public class WaitForCollect : ComponentIdConditionBase
    {
        public override void Bind(TaskRuntime ctx)
        {
            taskContext = ctx;
            TaskEvents.ObjectCollected += OnCollected;
        }

        public override void Unbind(TaskRuntime ctx) => TaskEvents.ObjectCollected -= OnCollected;

        private void OnCollected(TaskId id)
        {
            if (id.Equals(componentId))
                taskContext.Service.RequestStepComplete();
        }

        public override bool IsMet(TaskRuntime ctx) => ctx.Service.HasCollected(UsedComponentId);
    }

    [Serializable]
    public class WaitForUiEvent : ComponentIdConditionBase
    {
        private bool done;

        public override void Bind(TaskRuntime ctx)
        {
            taskContext = ctx;
            done = false;
            TaskEvents.UiEvent += OnUiEvent;
        }

        public override void Unbind(TaskRuntime ctx) => TaskEvents.UiEvent -= OnUiEvent;

        private void OnUiEvent(TaskId id)
        {
            if (!id.Equals(componentId)) return;
            done = true;
            taskContext.Service.RequestStepComplete();
        }

        public override bool IsMet(TaskRuntime ctx) => done;
    }

    [Serializable]
    public class WaitForUiClick : ComponentIdConditionBase
    {
        private bool clicked;

        public override void Bind(TaskRuntime ctx)
        {
            taskContext = ctx;
            clicked = false;
            TaskEvents.UiClicked += OnClick;
        }

        public override void Unbind(TaskRuntime ctx) => TaskEvents.UiClicked -= OnClick;

        private void OnClick(TaskId id)
        {
            if (!id.Equals(componentId)) return;
            clicked = true;
            taskContext.Service.RequestStepComplete();
        }

        public override bool IsMet(TaskRuntime ctx) => clicked;
    }

    [Serializable]
    public class WaitForBuildByPart : ComponentIdConditionBase
    {
        private bool done;

        public override void Bind(TaskRuntime ctx)
        {
            taskContext = ctx;
            done = false;
            TaskEvents.ObjectBuilt += OnBuild;
        }

        public override void Unbind(TaskRuntime ctx) => TaskEvents.ObjectBuilt -= OnBuild;

        private void OnBuild(TaskId id)
        {
            if (!id.Equals(componentId)) return;
            done = true;
            taskContext.Service.RequestStepComplete();
        }

        public override bool IsMet(TaskRuntime ctx) => done;
    }

    [Serializable]
    public class WaitForInteractClick : ComponentIdConditionBase
    {
        public WaitOptions options = WaitOptions.HandVisibility | WaitOptions.WorldArrow;

        private Connectable interactConnection;
        private bool done;

        public override void Bind(TaskRuntime ctx)
        {
            interactConnection = new Connectable();
            done = false;

            if (options.HasFlag(WaitOptions.HandVisibility))
                ctx.UiHand.VisibilityState(false);

            var t = ctx.ResolveTransform(componentId);
            if (t == null) return;

            if (options.HasFlag(WaitOptions.WorldArrow))
                ctx.WorldArrow.Show(t);

            if (!t.TryGetComponent(out ITargetObject target)) return;

            interactConnection += target.OnInteractedEvent.Subscribe(() =>
            {
                done = true;
                if (options.HasFlag(WaitOptions.HandVisibility)) ctx.UiHand.VisibilityState(false);
                if (options.HasFlag(WaitOptions.WorldArrow)) ctx.WorldArrow.DisableArrow();
                ctx.Service.RequestStepComplete();
            });

            interactConnection += target.Highlighted.Subscribe(state => SetHighlightedState(ctx, state, t));

            if (target.Highlighted.value)
                OnboardingDelay.Call(0.1f, () => SetHighlightedState(ctx, target.Highlighted.value, t));
            else
                ctx.UiHand.VisibilityState(false);
        }

        private void SetHighlightedState(TaskRuntime ctx, bool state, Transform t)
        {
            if (options.HasFlag(WaitOptions.HandVisibility)) ctx.UiHand.VisibilityState(state);

            if (options.HasFlag(WaitOptions.WorldArrow))
            {
                if (state) ctx.WorldArrow.Hide();
                else ctx.WorldArrow.Show(t);
            }
        }

        public override void Unbind(TaskRuntime ctx)
        {
            if (options.HasFlag(WaitOptions.HandVisibility)) ctx.UiHand.VisibilityState(false);
            if (options.HasFlag(WaitOptions.WorldArrow)) ctx.WorldArrow.DisableArrow();

            interactConnection?.Dispose();
            interactConnection = null;
        }

        public override bool IsMet(TaskRuntime ctx) => done || IsTargetMissingOrInactive(ctx);
    }

    [Serializable]
    public class WaitForInteract : ComponentIdConditionBase
    {
        public WaitOptions options = WaitOptions.HandVisibility | WaitOptions.WorldArrow;

        private Connectable interactConnection;
        private bool done;

        public override void Bind(TaskRuntime ctx)
        {
            interactConnection = new Connectable();
            done = false;

            if (options.HasFlag(WaitOptions.HandVisibility))
                ctx.UiHand.VisibilityState(false);

            var t = ctx.ResolveTransform(componentId);
            if (t == null) return;

            if (options.HasFlag(WaitOptions.WorldArrow))
                ctx.WorldArrow.Show(t);

            if (!t.TryGetComponent(out ITargetObject target)) return;

            interactConnection += target.Highlighted.Subscribe(state => SetHighlightedState(ctx, state, t, target));

            if (target.Highlighted.value)
                OnboardingDelay.Call(0.1f, () => SetHighlightedState(ctx, target.Highlighted.value, t, target));
            else
                ctx.UiHand.VisibilityState(false);
        }

        private void SetHighlightedState(TaskRuntime ctx, bool state, Transform t, ITargetObject target)
        {
            done = state || !target.GetInstanceOfObject.activeSelf;

            if (options.HasFlag(WaitOptions.HandVisibility)) ctx.UiHand.VisibilityState(state);

            if (options.HasFlag(WaitOptions.WorldArrow))
            {
                if (state) ctx.WorldArrow.Hide();
                else ctx.WorldArrow.Show(t);
            }

            if (done) ctx.Service.RequestStepComplete();
        }

        public override void Unbind(TaskRuntime ctx)
        {
            if (options.HasFlag(WaitOptions.HandVisibility)) ctx.UiHand.VisibilityState(false);
            if (options.HasFlag(WaitOptions.WorldArrow)) ctx.WorldArrow.DisableArrow();

            interactConnection?.Dispose();
            interactConnection = null;
        }

        public override bool IsMet(TaskRuntime ctx) => done || IsTargetMissingOrInactive(ctx);
    }
}
