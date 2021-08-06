using Prism.Events;
using HLCS01.Shared.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.ServiceCore.PubSubEvents
{
    public class OnForwardMessageRequestEvent : PubSubEvent<IMessage>
    {
    }
}
