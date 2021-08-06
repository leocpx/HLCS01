using HLCS01.Shared.Communication;

namespace HLCS01.Shared.Serializer
{
    public interface ITransportMessage : IMessage
    {
        IMessage ToMessage();
    }
}