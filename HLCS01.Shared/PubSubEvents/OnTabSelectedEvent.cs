using Prism.Events;
using Prism.Mvvm;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Shared.PubSubEvents
{
    public class OnTabSelectedEvent: PubSubEvent<int>
    {
    }
}
