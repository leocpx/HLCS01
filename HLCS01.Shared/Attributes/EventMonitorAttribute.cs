using HLCS01.Shared.Communication;
using System;

namespace HLCS01.Shared.Attributes
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
