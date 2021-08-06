using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R0013.Shared.Communication
{
    public interface IMessage
    {
        int Id { get; set; }
        string Value { get; set; }
        byte[] Data { get; set; }
        string ParameterName { get; set; }
    }
}
