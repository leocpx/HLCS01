using Prism.Ioc;
using R0013.Shared.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R0013.Shared.Communication
{
    public static class R0013HMIBootStrapperSetup
    {
        public static void ConfigureClientCommSocket(IContainerRegistry containerRegistry)
        {
            // MQTT
            containerRegistry.RegisterSingleton<ICommSocket, DummyMqttClient>();

            // SUPER
            //containerRegistry.RegisterSingleton<ICommSocket, SuperSocket.SuperClient>();
        }




        public static void ConfigureServerCommSocket(IContainerRegistry containerRegistry)
        {
            // MQTT
            containerRegistry.RegisterSingleton<ICommSocket, DummyMqttServer>();

            // SUPER
            //containerRegistry.RegisterSingleton<ICommSocket, SuperSocket.SuperServer>();
        }




        public static void RegisterSerializerType(IContainerRegistry containerRegistry)
        {
            // MPACK
            containerRegistry.RegisterSingleton<ICommSerializer, MPackCommSerializer<MPackTransportMessage>>();
            containerRegistry.RegisterSingleton<IUnivSerializer, MPackCommSerializer<MPackTransportMessage>>();

            // XML
            //containerRegistry.RegisterSingleton<ICommSerializer, XmlCommSerializer<XmlTransportMessage>>();
        }
    }


}
