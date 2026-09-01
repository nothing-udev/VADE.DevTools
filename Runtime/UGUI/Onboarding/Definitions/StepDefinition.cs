using System;
using System.Collections.Generic;
using UnityEngine;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public class StepDefinition
    {
        [GeneratedId] public string key;

        [TextArea(2, 4)] public string description;

        public float cooldownAfterStep;

        public bool completeWhenMet = true;
        public bool disableWhenAwake;

        [SerializeReference, SerializeReferenceList(typeof(IAction))]
        public List<IAction> onAction = new();

        [SerializeReference, SerializeReferenceList(typeof(ICondition))]
        public List<ICondition> conditions = new();

        public bool AreConditionsMet(TaskRuntime ctx)
        {
            foreach (var c in conditions)
                if (!c.IsMet(ctx)) return false;
            return true;
        }
    }
}
