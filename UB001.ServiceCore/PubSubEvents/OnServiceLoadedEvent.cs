﻿using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R0013.ServiceCore.PubSubEvents
{
    public class OnServiceLoadedEvent : PubSubEvent<IService>
    {
    }
}
