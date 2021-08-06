using Prism.Events;
using HLCS01.Shared.PubSubEvents;
using System;

namespace HLCS01.Shared.Communication
{
    public class DummyMqttServer : ICommSocket
    {
        #region -- PROPERTIES --
        #region -- PUBLIC --
        public event OnMessageReceivedEventHandler OnMessageReceived;
        #endregion
        #region -- PRIVATE --
        private Server _server { get; set; }
        private string _serverName { get; set; }
        private IEventAggregator _eventAggregator { get; set; }

        private string User { get; set; } = MqttCommSettings.User;
        private string Key { get; set; } = MqttCommSettings.Key;
        private int Port { get; set; } = MqttCommSettings.ServerPort;
        private string ClientTopic { get; set; } = MqttCommSettings.ClientTopic;
        #endregion
        #endregion

        #region -- CONSTRUCTOR --
        public DummyMqttServer(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _server = new Server("DummyMqttServer",User,Key,Port);
            _server.OnMessageReceived += OnMessageReceivedEventHandler;
            _eventAggregator.GetEvent<OnCloseEvent>().Subscribe(Stop);
        }
        #endregion

        #region -- FUNCTIONS --
        #region -- PUBLIC --
        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server?._server?.StopAsync();
        }
        public void Send(byte[] message)
        {
            _server.Send(ClientTopic, message);
        }
        #endregion
        #region -- PRIVATE --
        private void OnMessageReceivedEventHandler(byte[] message)
        {
            OnMessageReceived?.Invoke(message);
        }
        #endregion
        #endregion
    }
}
