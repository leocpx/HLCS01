using ImpromptuInterface;
using MessagePack;
using HLCS01.Shared.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Shared.Serializer
{
    [MessagePack.MessagePackObject]
    public class MPackTransportMessage : ITransportMessage
    {
        [Key(0)]
        public int Id { get; set; }
        
        [Key(1)]
        public string ParameterName { get; set; }

        [Key(2)]
        public string Value { get; set; }

        [Key(3)]
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

        public MPackTransportMessage()
        {

        }
    }
}
