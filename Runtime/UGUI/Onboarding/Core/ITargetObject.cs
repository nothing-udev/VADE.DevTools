using UnityEngine;
using UnityEngine.Events;
using VADE.DevTools.Reactive;

namespace VADE.DevTools.Onboarding
{
    public interface ITargetObject
    {
        GameObject GetInstanceOfObject { get; }
        UnityEvent OnInteractedEvent { get; }
        Reactive<bool> Highlighted { get; }
    }
}
