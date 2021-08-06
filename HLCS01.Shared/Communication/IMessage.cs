using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Shared.Communication
{
    public interface IMessage
    {
        int Id { get; set; }
        string Value { get; set; }
        byte[] Data { get; set; }
        string ParameterName { get; set; }
    }
}
