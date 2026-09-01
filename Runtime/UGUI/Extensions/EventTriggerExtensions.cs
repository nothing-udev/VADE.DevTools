using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace VADE.DevTools.Extensions
{
    public static class EventTriggerExtensions
    {
        public static void AddEvent(this EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(callback);
            trigger.triggers.Add(entry);
        }
    }
}
