using System;

namespace R0013.ServiceCore.Models
{
    public class MessageConsumerInstruction
    {
        public string ParameterName { get; set; }
        public Func<string, byte[], object> ProcessMessage= (v, d) => "";
        public bool ResponseRequired { get; set; }

    }
}
