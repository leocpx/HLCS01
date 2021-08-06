using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Shared.Attributes
{
    /// <summary>
    /// Marks a string wpf binded property to be remotely-updated by dataprovider
    /// needs to specify the parameter unique address and update interval in ms
    /// </summary>
    public class DefaultMonitorAttribute : Attribute
    {
        public string ParameterName;
        public int UpdateInterval;
      

        public DefaultMonitorAttribute(string parameterName, int updateInterval)
        {
            ParameterName = parameterName;
            UpdateInterval = updateInterval;
        }
    }
}
