using ImpromptuInterface;
using HLCS01.Shared.Communication;

namespace HLCS01.Shared.Serializer
{
    public class XmlTransportMessage : ITransportMessage
    {
        public int Id { get; set; }
        public string ParameterName { get; set; }
        public string Value { get; set; }
        public byte[] Data { get; set; }
        public IMessage ToMessage()
        {
            return new
            {
                Id = Id,
                ParameterName = ParameterName,
                Value = Value,
                Data = Data
            }.ActLike<IMessage>();
        }

        public XmlTransportMessage()
        {
        }
    }
}
