using System;
using System.Collections.Generic;
using UnityEngine;
using VADE.DevTools.Attributes;

namespace VADE.DevTools.Onboarding
{
    [Serializable]
    public class TaskDefinition
    {
        [GeneratedId] public string key;

        public string title;

        [TextArea] public string description;

        public List<StepDefinition> steps = new();
    }
}
