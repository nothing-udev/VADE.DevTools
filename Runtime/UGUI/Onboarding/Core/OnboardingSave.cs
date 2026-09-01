using System;
using System.Collections.Generic;
using VADE.DevTools.Persistence;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public struct OnboardingProgress
    {
        public int task;
        public int step;
        public bool completed;
    }

    public static class OnboardingSave
    {
        private static readonly AutoSave<OnboardingProgress> progress =
            new("vade_onboarding_progress", AutoSaveType.PlayerPrefs, new OnboardingProgress { task = -1 });

        private static readonly AutoSave<List<string>> collected =
            new("vade_onboarding_collected", AutoSaveType.PlayerPrefs, new List<string>());

        public static bool HasOnboardingSaves() => progress.value.task >= 0;

        public static void Save(int task, int step, bool completed = false) =>
            progress.value = new OnboardingProgress { task = task, step = step, completed = completed };

        public static bool TryLoad(out int task, out int step, out bool completed)
        {
            var v = progress.value;
            task = v.task;
            step = v.step;
            completed = v.completed;
            return v.task >= 0;
        }

        public static void Delete()
        {
            progress.value = new OnboardingProgress { task = -1 };
            progress.Delete();
            collected.value = new List<string>();
            collected.Delete();
        }

        public static bool HasCollected(string id) => collected.value.Contains(id);

        public static void AddCollected(string id)
        {
            if (collected.value.Contains(id)) return;
            collected.value.Add(id);
            collected.Flush();
        }

        public static void RemoveCollected(string id)
        {
            if (!collected.value.Remove(id)) return;
            collected.Flush();
        }
    }
}
