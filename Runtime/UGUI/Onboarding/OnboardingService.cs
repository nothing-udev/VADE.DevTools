using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VADE.DevTools.Attributes;
using VADE.DevTools.DI;
using VADE.DevTools.Events;
using VADE.DevTools.Reactive;
#if VADE_DOTWEEN
using DG.Tweening;
#endif

namespace VADE.DevTools.Onboarding
{
    public class OnboardingService : MonoBehaviour
    {
        public static OnboardingService Instance { get; private set; }

        [SerializeField] private OnboardingAsset onboarding;

        private int taskIndex;
        private int stepIndex;
        private bool running;
        private bool waitingCooldown;

        private TaskRuntime ctx;

        private readonly Dictionary<TaskId, TaskComponentBase> components = new();
        private readonly Dictionary<TaskId, Transform> uiById = new();
        private readonly HashSet<string> completedDisabledStepsUUID = new();

        public string CurrentStepUID { get; private set; }
        public bool IsOnboardingRunning => running;
        public TaskRuntime Ctx => ctx;
        public int StepId => stepIndex;

        private readonly UnityEvent<TaskComponentBase> onRegistered = new();
        public readonly UnityEvent OnOnboardingComplete = new();
        public readonly Reactive<bool> IsOnboardingCompleted = new(false);
        public event Action<int, StepDefinition> StepCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DI.Register(this);

            ctx = new TaskRuntime { Service = this };
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                if (DI.IsRegistered<OnboardingService>())
                    DI.Unregister<OnboardingService>();
            }
        }

        private void OnEnable() => SubscribeEvents(true);
        private void OnDisable() => SubscribeEvents(false);

        private void SubscribeEvents(bool subscribe)
        {
            if (subscribe)
            {
                TaskEvents.ComponentCompleted += OnStepRelevantEvent;
                TaskEvents.UiClicked += OnStepRelevantEvent;
                TaskEvents.UiEvent += OnStepRelevantEvent;
                TaskEvents.ObjectCollected += OnObjectCollected;
            }
            else
            {
                TaskEvents.ComponentCompleted -= OnStepRelevantEvent;
                TaskEvents.UiClicked -= OnStepRelevantEvent;
                TaskEvents.UiEvent -= OnStepRelevantEvent;
                TaskEvents.ObjectCollected -= OnObjectCollected;
            }
        }

        public void SetupUIHand(UiHandPointer hand) => ctx.UiHand = hand;
        public void SetupWorldArrow(WorldArrowPointer arrow) => ctx.WorldArrow = arrow;

        [EditorButton]
        public void StartOnboarding()
        {
            if (onboarding == null || onboarding.tasks.Count == 0)
            {
                Debug.LogWarning("[OnboardingService] Onboarding asset missing or empty");
                return;
            }

            running = true;
            LoadProgressOrReset();
            IsOnboardingCompleted.value = !running;

            if (!running) return;
            EnterStep();
        }

        [EditorButton]
        public void CompleteOnboardingInstantly()
        {
            if (onboarding == null || onboarding.tasks.Count == 0)
            {
                Debug.LogWarning("[OnboardingService] Onboarding asset missing or empty");
                return;
            }

            ActivateViews(onboarding.tasks.Count - 1, onboarding.tasks[^1].steps.Count - 1);

            taskIndex = onboarding.tasks.Count;
            stepIndex = 0;
            running = false;
            waitingCooldown = false;

            OnboardingSave.Save(onboarding.tasks.Count, 0, true);
        }

        [EditorButton]
        public void SkipCurrentStep()
        {
            if (!running)
            {
                Debug.LogWarning("[OnboardingService] Onboarding is not running.");
                return;
            }

            var step = CurrentStep();
            if (step == null)
            {
                Debug.LogWarning("[OnboardingService] No current step to skip.");
                return;
            }

            if (step.disableWhenAwake)
            {
                completedDisabledStepsUUID.Add(step.key);

                foreach (var a in step.onAction)
                {
                    if (a is IUsesComponent uses && components.TryGetValue(uses.UsedComponentId, out var comp) && comp is IVisibleComponent view)
                        view.Show();
                }
            }

            ExecuteStepActions(step, true);
            ExitStep(step);
            AdvanceStep();

            if (CurrentTask() == null || CurrentStep() == null)
            {
                OnboardingSave.Save(onboarding.tasks.Count, 0, true);
                running = false;
                completedDisabledStepsUUID.Clear();
                return;
            }

            OnboardingSave.Save(taskIndex, stepIndex);
            EnterStep();
        }

        [EditorButton]
        public void MoveTo(int task, int step)
        {
            ExitStep();
            taskIndex = Mathf.Clamp(task - 1, 0, onboarding.tasks.Count - 1);
            stepIndex = Mathf.Clamp(step - 1, 0, onboarding.tasks[taskIndex].steps.Count - 1);
            EnterStep();
            OnboardingSave.Save(taskIndex, stepIndex);
        }

        [EditorButton] public void Pause() => running = false;
        [EditorButton] public void Resume() => running = true;
        [EditorButton] public void NextStep() => CompleteStep();
        [EditorButton] public void DeleteSaves() => OnboardingSave.Delete();

        public void Register(TaskComponentBase comp)
        {
            if (components.ContainsKey(comp.Id))
            {
                components[comp.Id] = comp;
                onRegistered?.Invoke(comp);
                return;
            }

            components.Add(comp.Id, comp);
            var t = comp.GetComponent<Transform>();
            if (t != null) uiById.Add(comp.Id, t);

            onRegistered?.Invoke(comp);

            var step = CurrentStep();
            if (step == null) return;

            if (step.onAction.Exists(c => c.UsedComponentId == comp.Id))
            {
                comp.enabled = true;
                return;
            }

            if (comp is IVisibleComponent hideView)
                hideView.Hide();

            comp.enabled = false;
        }

        public void Unregister(TaskComponentBase comp)
        {
            components.Remove(comp.Id);
            uiById.Remove(comp.Id);
        }

        private void ActivateStepHandler(StepDefinition step)
        {
            ActivateStepActionHandler(step);
            ActivateStepConditionHandler(step);
        }

        private void ActivateStepConditionHandler(StepDefinition step)
        {
            if (step == null) return;
            foreach (var c in step.conditions)
                if (components.TryGetValue(c.UsedComponentId, out var comp))
                    comp.enabled = true;
        }

        private void ActivateStepActionHandler(StepDefinition step, bool onlyView = false)
        {
            if (step == null) return;

            foreach (var a in step.onAction)
            {
                if (a is IUsesComponent uses && components.TryGetValue(uses.UsedComponentId, out var compAction))
                {
                    compAction.enabled = !onlyView;
                    if (compAction is IVisibleComponent view) view.Show();
                }
            }
        }

        private void DeactivateStepHandler(StepDefinition step)
        {
            DeactivateStepConditionHandler(step);
            DeactivateStepActionHandler(step);
        }

        private void DeactivateStepConditionHandler(StepDefinition step)
        {
            if (step == null) return;
            foreach (var c in step.conditions)
                if (components.TryGetValue(c.UsedComponentId, out var comp))
                    comp.enabled = false;
        }

        private void DeactivateStepActionHandler(StepDefinition step)
        {
            if (step == null) return;

            foreach (var a in step.onAction)
            {
                if (a is IUsesComponent uses && components.TryGetValue(uses.UsedComponentId, out var compAction))
                {
                    if (step.disableWhenAwake && compAction is IVisibleComponent view && !HasDisabledStepCompleted(step.key))
                        view.Hide();

                    compAction.enabled = false;
                }
            }
        }

        public Transform ResolveTransform(TaskId id) => components.TryGetValue(id, out var c) ? c.transform : null;
        public RectTransform ResolveUI(TaskId id) => uiById.TryGetValue(id, out var t) ? t as RectTransform : null;

        public bool HasCollected(TaskId id) => OnboardingSave.HasCollected(id.Value);

        private void OnObjectCollected(TaskId id)
        {
            OnboardingSave.AddCollected(id.Value);
            RequestStepComplete();
        }

        private void LoadProgressOrReset()
        {
            if (OnboardingSave.TryLoad(out var task, out var step, out var completed))
            {
                if (completed && task >= onboarding.tasks.Count)
                {
                    running = false;
                    ActivateViews(onboarding.tasks.Count - 1, onboarding.tasks[^1].steps.Count - 1);
                    return;
                }

                taskIndex = Mathf.Clamp(task, 0, onboarding.tasks.Count - 1);
                stepIndex = Mathf.Clamp(step, 0, onboarding.tasks[taskIndex].steps.Count - 1);
                ActivateViews(taskIndex, stepIndex);
            }
            else
            {
                taskIndex = stepIndex = 0;
            }
        }

        private void ActivateViews(int taskIdx, int stepIdx)
        {
            completedDisabledStepsUUID.Clear();

            for (int t = 0; t < onboarding.tasks.Count; t++)
            {
                if (t > taskIdx) continue;
                for (int s = 0; s < onboarding.tasks[t].steps.Count; s++)
                {
                    if (s <= stepIdx)
                        ActivateStepActionHandler(onboarding.tasks[t].steps[s], true);
                }
            }
        }

        private TaskDefinition CurrentTask() => onboarding != null && onboarding.tasks.IsValidIndex(taskIndex) ? onboarding.tasks[taskIndex] : null;
        private StepDefinition CurrentStep() => CurrentTask()?.steps.IsValidIndex(stepIndex) == true ? CurrentTask().steps[stepIndex] : null;

        private void EnterStep()
        {
            var step = CurrentStep();
            if (step == null)
            {
                running = false;
                return;
            }

            ctx.CurrentTask = CurrentTask();
            ctx.CurrentStepIndex = stepIndex;
            CurrentStepUID = step.key;

            if (!AreAllComponentsReady(step))
            {
                Connectable stepConnection = new();
                stepConnection += onRegistered.Subscribe(_ =>
                {
                    if (AreAllComponentsReady(step))
                    {
                        stepConnection.Dispose();
                        EnterStep();
                    }
                });
                return;
            }

            ActivateStepHandler(step);
            BindStep(step);
            ExecuteStepActions(step, true);

            if (step.completeWhenMet && step.AreConditionsMet(ctx))
                CompleteStep();
        }

        private bool AreAllComponentsReady(StepDefinition step)
        {
            foreach (var act in step.onAction)
            {
                if (act is IConfigWithoutId || HasCollected(act.UsedComponentId)) continue;
                if (!components.ContainsKey(act.UsedComponentId)) return false;
            }

            foreach (var cond in step.conditions)
            {
                if (cond is IConfigWithoutId || HasCollected(cond.UsedComponentId)) continue;
                if (!components.ContainsKey(cond.UsedComponentId)) return false;
            }

            return true;
        }

        private void ExitStep()
        {
            ExitStep(CurrentStep());
        }

        private void ExitStep(StepDefinition step)
        {
            if (step == null) return;
            ExecuteStepActions(step, false);
            UnbindStep(step);
            DeactivateStepHandler(step);
        }

        private void BindStep(StepDefinition step)
        {
            foreach (var c in step.conditions) c.Bind(ctx);
        }

        private void UnbindStep(StepDefinition step)
        {
            foreach (var c in step.conditions) c.Unbind(ctx);
        }

        private void ExecuteStepActions(StepDefinition step, bool enter)
        {
            foreach (var a in step.onAction)
                if (enter) a.Enter(ctx); else a.Exit(ctx);
        }

        public void RequestStepComplete()
        {
            if (!running) return;
            var step = CurrentStep();
            if (step != null && step.completeWhenMet && step.AreConditionsMet(ctx))
                CompleteStep();
        }

        private void CompleteStep()
        {
            if (!running || waitingCooldown) return;

            var step = CurrentStep();
            float cooldown = step?.cooldownAfterStep ?? 0f;

            StepCompleted?.Invoke(stepIndex, step);
            EventBus.Publish(new StepCompletedEvent(stepIndex, step));

            foreach (var cond in step.conditions)
                if (cond is WaitForCollect wfc)
                    OnboardingSave.RemoveCollected(wfc.UsedComponentId.Value);

            if (step.disableWhenAwake)
                completedDisabledStepsUUID.Add(step.key);

            ExitStep(step);
            AdvanceStep();

            if (CurrentTask() == null || CurrentStep() == null)
            {
                OnboardingSave.Save(onboarding.tasks.Count, 0, true);
                running = false;
                completedDisabledStepsUUID.Clear();
                OnOnboardingComplete?.Invoke();
                EventBus.Publish(new OnboardingCompletedEvent());
                return;
            }

            OnboardingSave.Save(taskIndex, stepIndex);

            if (cooldown > 0f)
            {
                waitingCooldown = true;
                ScheduleDelayedCall(cooldown, () =>
                {
                    waitingCooldown = false;
                    EnterStep();
                });
            }
            else
            {
                EnterStep();
            }
        }

        private bool HasDisabledStepCompleted(string id) => completedDisabledStepsUUID.Contains(id);

        private void AdvanceStep()
        {
            stepIndex++;
            if (stepIndex >= CurrentTask()?.steps.Count)
            {
                taskIndex++;
                stepIndex = 0;
            }
        }

        private void OnStepRelevantEvent(TaskId id)
        {
            var step = CurrentStep();
            if (step != null && step.completeWhenMet && step.AreConditionsMet(ctx))
                CompleteStep();
        }

        private void ScheduleDelayedCall(float delay, Action action)
        {
#if VADE_DOTWEEN
            DOVirtual.DelayedCall(delay, () => action());
#else
            StartCoroutine(DelayedCallRoutine(delay, action));
#endif
        }

#if !VADE_DOTWEEN
        private IEnumerator DelayedCallRoutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action();
        }
#endif

        public string GetDebugInfo() =>
            $"Task {taskIndex + 1}/{onboarding?.tasks.Count ?? 0}, " +
            $"Step {stepIndex + 1}/{(CurrentTask()?.steps.Count ?? 0)}, " +
            $"Cooldown: {(waitingCooldown ? "WAITING" : "READY")}";
    }
}
