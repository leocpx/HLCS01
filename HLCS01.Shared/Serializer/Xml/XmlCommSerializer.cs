using ImpromptuInterface;
using HLCS01.Shared.Serializer;
using System.IO;
using System.Xml.Serialization;

namespace HLCS01.Shared.Communication
{
    public class XmlCommSerializer<T> : ICommSerializer where T:ITransportMessage, new()
    {

        public XmlCommSerializer()
        {
        }

        public IMessage Deserialize(byte[] data)
        {
            var transportMessage = _getXmlTransportMessage(data);
            return transportMessage.ToMessage();
        }

        private T _getXmlTransportMessage(byte[] raw)
        {
            var stream = new MemoryStream(raw);
            var ser = new XmlSerializer(typeof(T));
            return (T)ser.Deserialize(stream);
        }

        public byte[] Serialize(IMessage value)
        {
            var xmlMessage = new T()
            {
                Id = value.Id,
                ParameterName = value.ParameterName,
                Value = value.Value,
                Data = value.Data
            };

            var stream = new MemoryStream();
            var ser = new XmlSerializer(typeof(T));
            ser.Serialize(stream, xmlMessage);
            return stream.ToArray();
        }

        public byte[] Serialize<T1>(T1 data)
        {
            var stream = new MemoryStream();
            var ser = new XmlSerializer(typeof(T1));
            ser.Serialize(stream, data);
            return stream.ToArray();
        }

        public T1 Deserialize<T1>(byte[] data)
        {
            var stream = new MemoryStream(data);
            var ser = new XmlSerializer(typeof(T));
            return (T1)ser.Deserialize(stream);
        }
    }
}