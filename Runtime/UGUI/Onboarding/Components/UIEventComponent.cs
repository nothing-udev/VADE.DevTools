using UnityEngine;
using UnityEngine.EventSystems;
using VADE.DevTools.Extensions;

namespace VADE.DevTools.Onboarding
{
    [RequireComponent(typeof(EventTrigger))]
    public class UIEventComponent : TaskComponentBase
    {
        [SerializeField] private EventTriggerType eventType;
        private EventTrigger eventTrigger;

        private void Awake()
        {
            eventTrigger = gameObject.GetComponent<EventTrigger>();
            eventTrigger?.AddEvent(eventType, OnEventCall);
        }

        private void OnEventCall(BaseEventData data) => TaskEvents.RaiseUiEvent(Id);
    }
}
