using System;
using UnityEngine;

namespace VADE.DevTools.Onboarding
{
    public static class TaskEvents
    {
        public static event Action<TaskId> ComponentCompleted;
        public static event Action<TaskId, float> ComponentProgress;

        public static event Action<TaskId> UiClicked;
        public static event Action<TaskId> UiEvent;

        public static event Action<TaskId> ObjectCollected;
        public static event Action<TaskId> ObjectBuilt;

        public static void RaiseCompleted(TaskId id) => ComponentCompleted?.Invoke(id);
        public static void RaiseProgress(TaskId id, float p) => ComponentProgress?.Invoke(id, Mathf.Clamp01(p));
        public static void RaiseUiClick(TaskId id) => UiClicked?.Invoke(id);
        public static void RaiseUiEvent(TaskId id) => UiEvent?.Invoke(id);
        public static void RaiseObjectCollected(TaskId id) => ObjectCollected?.Invoke(id);
        public static void RaiseObjectBuilt(TaskId id) => ObjectBuilt?.Invoke(id);
    }
}
