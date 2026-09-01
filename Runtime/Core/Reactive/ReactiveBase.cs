using System;
using UnityEngine;

namespace VADE.DevTools.Reactive
{

    public abstract class ReactiveBehaviour : MonoBehaviour
    {
        protected readonly Connectable Connections = new();

        protected virtual void OnDestroy() => Connections.Dispose();
    }

    public abstract class ReactiveObject : IDisposable
    {
        protected readonly Connectable Connections = new();

        public virtual void Dispose() => Connections.Dispose();
    }
}
