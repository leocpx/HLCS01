using R0013.Shared.Serializer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R0013.Shared.Communication
{
    public delegate void OnMessageReceivedEventHandler(byte[] message);

    public interface ICommSocket
    {
        void Start();
        void Stop();
        void Send(byte[] obj);
        event OnMessageReceivedEventHandler OnMessageReceived;
    }

    public enum TargetDevice
    { 
        PLC,
        Pace5000,
        Pace1000,
        TEC,
        Tester,
        Handler
    }
}
