using ImpromptuInterface;

namespace R0013.Shared.Communication
{
    public interface ICommSerializer
    {
        byte[] Serialize(IMessage msg);

        IMessage Deserialize(byte[] data);
    }

    public interface IUnivSerializer
    {
        byte[] _Serialize<T>(T data);
        T _Deserialize<T>(byte[] data);
    }
}