using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using MQTTnet.Server;
using HLCS01.Shared.Serializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HLCS01.Shared.Communication
{
    public class DummyMqttClient : ICommSocket
    {
        #region -- PROPERTIES --
        public event OnMessageReceivedEventHandler OnMessageReceived;

        private MqttClient _commSocket { get; set; }
        private string ClientName { get; set; } = MqttCommSettings.ClientName;
        private string ListenTopic { get; set; } = MqttCommSettings.ClientTopic;
        private string SupervisorTopic { get; set; } = MqttCommSettings.SupervisorTopic;
        private string User { get; set; } = MqttCommSettings.User;
        private string Key { get; set; } = MqttCommSettings.Key;
        private string Address { get; set; } = MqttCommSettings.ServerAddress;
        private int Port { get; set; } = MqttCommSettings.ServerPort;
        #endregion

        #region -- CONSTRUCTOR --
        public DummyMqttClient()
        {
            Server.quality = 2;
            _commSocket = new MqttClient(ClientName, User, Key, Address, Port);
            _commSocket.Subscribe(ListenTopic);
            _commSocket.Start();
            _commSocket.OnMessageReceived = (data) => OnMessageReceived?.Invoke(data);
        }

        ~DummyMqttClient()
        {
        }
        #endregion

        #region -- PROPERTIES --
        #region -- PUBLIC --
        public void Start()
        {
            _commSocket.Start();
        }
        public void Stop()
        {
            _commSocket.mqttClient.StopAsync();
        }
        public void Send(byte[] message)
        {
            _commSocket.Send(SupervisorTopic, message);
        }

        #endregion
        #region -- PRIVATE --

        #endregion
        #endregion

    }

    #region -- MQTT SERVER / CLIENT --
    public class MqttClient
    {
        public IManagedMqttClient mqttClient;
        private ManagedMqttClientOptions _options;
        private string _name;
        public Action<byte[]> OnMessageReceived;
        public MqttClient(string name, string user, string key, string address, int port)
        {
            _name = name;
            var options = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(new MqttClientOptionsBuilder()
                    .WithClientId(_name+DateTime.Now.ToString("fffssmmHHMMyyyy"))
                    .WithTcpServer(address, port)
                    .WithCredentials(user, key)
                    //.WithKeepAlivePeriod(TimeSpan.FromSeconds(1))
                    .WithCommunicationTimeout(TimeSpan.FromSeconds(1))
                    //.WithRequestProblemInformation(true)
                    //.WithRequestResponseInformation(true)
                    .Build())
                .Build();

            _options = options;
            mqttClient = new MqttFactory().CreateManagedMqttClient();

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                try
                {
                    string topic = e.ApplicationMessage.Topic;
                    e.AutoAcknowledge = true;
                    e.IsHandled = true;
                    if (string.IsNullOrWhiteSpace(topic) == false)
                    {
                        //string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        OnMessageReceived?.Invoke(e.ApplicationMessage.Payload);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message, ex);
                }
            });
        }
        public void Unsubscribe(string topic)
        {
            mqttClient.UnsubscribeAsync(topic);
        }
        public void Subscribe(string topic)
        {
            mqttClient.SubscribeAsync(topic);
        }
        public void Start()
        {
            mqttClient.StartAsync(_options);
        }
        public void Send(string topic, byte[] data)
        {
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(data)
                .WithMessageExpiryInterval(1)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            mqttClient.PublishAsync(msg);
        }
    }
    public class Server
    {
        /// <summary>
        /// The logger.
        /// </summary>
        public IMqttServer _server;
        private IMqttServerOptions _options;
        public static int quality = 0;
        public Action<string> OnClientConnected;
        public Action<string> OnClientDisconnected;
        public Action<byte[]> OnMessageReceived;

        public Server(string name, string user, string key, int port)
        {
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithClientId(name)
                .WithDefaultEndpointPort(port)
                .WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(1))
                .WithMaxPendingMessagesPerClient(2)
                .WithConnectionValidator(
                c =>
                {
                    if (c.Username != user)
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        LogMessage(c, true);
                        return;
                    }

                    if (c.Password != key)
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        LogMessage(c, true);
                        return;
                    }
                    OnClientConnected?.Invoke(c.ClientId);
                    c.ReasonCode = MqttConnectReasonCode.Success;

                    LogMessage(c, false);
                }).WithSubscriptionInterceptor(
                c =>
                {
                    c.AcceptSubscription = true;
                    LogMessage(c, true);
                }).WithApplicationMessageInterceptor(
                c =>
                {
                    c.AcceptPublish = true;
                    if(c.ClientId!=name)
                    {
                        OnMessageReceived?.Invoke(c.ApplicationMessage.Payload);
                    }
                })
                //.WithClientMessageQueueInterceptor(c => OnMessageReceived?.Invoke(c.ApplicationMessage.Payload))
                ;

            _server = new MqttFactory().CreateMqttServer();
            _options = optionsBuilder.Build();
        }

        public void Send(string topic, string txt)
        {
            _server.PublishAsync(new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(quality)
                .WithPayload(Encoding.UTF8.GetBytes(txt))
                .Build());
        }
        public void Send(string topic, byte[] message)
        {
            _server.PublishAsync(new MqttApplicationMessageBuilder()
                .WithAtLeastOnceQoS()
                .WithTopic(topic)
                .WithMessageExpiryInterval(1)
                .WithPayload(message)
                .Build());
        }
        public Task Start()
        {
            return _server.StartAsync(_options);
        }

        public void SendMessage(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(quality)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            _server.PublishAsync(message);
        }

        /// <summary> 
        ///     Logs the message from the MQTT subscription interceptor context. 
        /// </summary> 
        /// <param name="context">The MQTT subscription interceptor context.</param> 
        /// <param name="successful">A <see cref="bool"/> value indicating whether the subscription was successful or not.</param> 
        private static void LogMessage(MqttSubscriptionInterceptorContext context, bool successful)
        {
            if (context == null)
            {
                return;
            }

        }

        /// <summary>
        ///     Logs the message from the MQTT message interceptor context.
        /// </summary>
        /// <param name="context">The MQTT message interceptor context.</param>
        private static void LogMessage(MqttApplicationMessageInterceptorContext context)
        {
            if (context == null)
            {
                return;
            }

            var payload = Encoding.UTF8.GetString(context.ApplicationMessage.Payload);
            Console.WriteLine($"server received from client:{payload}");

        }

        /// <summary> 
        ///     Logs the message from the MQTT connection validation context. 
        /// </summary> 
        /// <param name="context">The MQTT connection validation context.</param> 
        /// <param name="showPassword">A <see cref="bool"/> value indicating whether the password is written to the log or not.</param> 
        private static void LogMessage(MqttConnectionValidatorContext context, bool showPassword)
        {
            if (context == null)
            {
                return;
            }

            if (showPassword)
            {

            }
            else
            {
            }
        }
    }
    #endregion
}
