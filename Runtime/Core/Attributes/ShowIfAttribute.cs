using UnityEngine;

namespace VADE.DevTools.Attributes
{

    public class ShowIfAttribute : PropertyAttribute
    {
        public readonly string Condition;
        public readonly object CompareValue;

        public ShowIfAttribute(string condition, object compareValue = null)
        {
            Condition = condition;
            CompareValue = compareValue ?? true;
        }
    }

    public class HideIfAttribute : ShowIfAttribute
    {
        public HideIfAttribute(string condition, object compareValue = null) : base(condition, compareValue) { }
    }
}
