using Prism.Events;
using R0013.Shared.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.PubSubEvents
{
    public class RegisterEvent : PubSubEvent
    {
        public void Subscribe()
        {
            throw new NotImplementedException();
        }
    }

    public class DataProviderRegisteredEvent : PubSubEvent<IDataProvider>
    {
        public IDataProvider _dataProvider;
    }
}
