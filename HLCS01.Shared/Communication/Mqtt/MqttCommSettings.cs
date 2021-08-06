using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Shared.Communication
{
    public static class MqttCommSettings
    {
        public static string ClientName { get; set; } = "DummyMqttClient";
        public static string ClientTopic { get; set; } = "R0013";
        public static string SupervisorTopic { get; set; } = "R0013-Supervisor";
        public static string ServerAddress { get; set; } = "172.18.4.138";
        public static int ServerPort { get; set; } = 1883;
        public static string User { get; set; } = "minotauro";
        public static string Key { get; set; } = "minotauro";
    }
}
