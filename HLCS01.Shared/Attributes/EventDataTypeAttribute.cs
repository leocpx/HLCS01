﻿using System;

namespace HLCS01.Shared.Attributes
{
    public class EventDataTypeAttribute : Attribute
    {
        public Type DataType { get; set; }
        public EventDataTypeAttribute(Type dataType)
        {
            DataType = dataType;
        }
    }
}
