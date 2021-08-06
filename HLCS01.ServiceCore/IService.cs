using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.ServiceCore
{
    public interface IService
    {
        void Start();
        void Stop();
        string ServiceName { get; set; }

        event OnServiceMessageEventHandler OnServiceMessage;
    }

    public delegate void OnServiceMessageEventHandler(IService sender, string serviceMessage);
}
