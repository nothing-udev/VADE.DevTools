using System;
using UnityEngine;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public struct TaskId : IEquatable<TaskId>
    {
        [SerializeField] public string value;
        public string Value => string.IsNullOrEmpty(value) ? "" : value;

        public TaskId(string v) { value = v; }

        public bool Equals(TaskId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is TaskId other && Equals(other);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override string ToString() => Value;

        public static TaskId NewGuid() => new(Guid.NewGuid().ToString("N"));
        public static implicit operator string(TaskId id) => id.Value;

        public static bool operator ==(TaskId a, TaskId b) => a.Equals(b);
        public static bool operator !=(TaskId a, TaskId b) => !a.Equals(b);
    }
}
