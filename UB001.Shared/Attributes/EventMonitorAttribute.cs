using R0013.Shared.Communication;
using System;

namespace R0013.Shared.Attributes
{
    public class EventMonitorAttribute : Attribute
    {
        public string EventName;
        public Type SerializedType;

        public EventMonitorAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}
