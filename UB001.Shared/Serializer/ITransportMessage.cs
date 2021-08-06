using R0013.Shared.Communication;

namespace R0013.Shared.Serializer
{
    public interface ITransportMessage : IMessage
    {
        IMessage ToMessage();
    }
}