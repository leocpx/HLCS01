using R0013.Shared.Serializer;

namespace R0013.Shared.Communication
{

    public class MPackCommSerializer<T> : ICommSerializer, IUnivSerializer where T : ITransportMessage, new()
    {

        public MPackCommSerializer()
        {
        }

        public IMessage Deserialize(byte[] data)
        {
            var transportMessage = _getDeserializedObject(data);
            return transportMessage.ToMessage();
        }

        private T _getDeserializedObject(byte[] raw)
        {
            var transportMessage = MessagePack.MessagePackSerializer.Deserialize<T>(raw);
            return transportMessage;
        }

        public byte[] Serialize(IMessage value)
        {
            var newTransportMessage = new T()
            {
                Id = value.Id,
                ParameterName = value.ParameterName,
                Value = value.Value,
                Data = value?.Data,
            };

            return MessagePack.MessagePackSerializer.Serialize(newTransportMessage);
        }

        public byte[] _Serialize<T1>(T1 data)
        {
            return MessagePack.MessagePackSerializer.Serialize<T1>(data);
        }

        public T1 _Deserialize<T1>(byte[] data)
        {
            return MessagePack.MessagePackSerializer.Deserialize<T1>(data);
        }
    }
}