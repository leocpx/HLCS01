using System;

namespace HLCS01.ServiceCore.Models
{
    public class MessageConsumerInstruction
    {
        public string ParameterName { get; set; }
        public Func<string, byte[], object> ProcessMessage= (v, d) => "";
        public bool ResponseRequired { get; set; }

    }
}
