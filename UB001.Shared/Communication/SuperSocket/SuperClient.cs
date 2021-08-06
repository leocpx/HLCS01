using Prism.Events;
using R0013.Shared.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MyFramework.TCP;

namespace MyFramework.TCP
{
    #region MyTcpEventArgs

    /// <summary>
    /// MyTcpEventArgs type is based on EventArgs, used as event data
    /// to store the TCP message and the client handler who sent the 
    /// message.
    /// </summary>
    public class MyTcpEventArgs : EventArgs
    {
        public MyTcpClient Client { get; set; }
        public string Message { get; set; }
        public byte[] Buffer { get; set; }
    }

    #endregion

    /// <summary>
    /// MyTcpListener is a TCP library part of the MyFramework used as 
    /// TCP server communication with the connected clients.
    /// </summary>
    public class MyTcpListener
    {
        #region PROPERTIES

        /// <summary>
        /// TcpListener type used as server object. To Open a socket and accept
        /// connections.
        /// </summary>
        public TcpListener Listener;



        /// <summary>
        /// List of connected TCP clients.
        /// </summary>
        public List<MyTcpClient> MyClients;



        /// <summary>
        /// Flag used to store status of the socket.
        /// </summary>
        public bool Opened { get; set; }



        /// <summary>
        /// Thread used to check for connections and messages.
        /// </summary>
        public Thread thread;



        /// <summary>
        /// Object used as semaphore to restrict access to one
        /// single thread at a time to StopRequest flag.
        /// </summary>
        public Object StopRequest_locker = new Object();



        /// <summary>
        /// Bool flag used to inform running threads to exit their loop
        /// and dispose.
        /// </summary>
        public bool stopRequest;



        /// <summary>
        /// Bool flag used to inform running threads to exit their loop
        /// and dispose.
        /// </summary>
        public bool StopRequest
        {
            get
            {
                lock (StopRequest_locker)
                {
                    return stopRequest;
                }
            }
            set
            {
                lock (StopRequest_locker)
                {
                    stopRequest = value;
                }
            }
        }



        /// <summary>
        /// Used to wait a specific amount of time before checking again
        /// if a client is still connected.
        /// </summary>
        public int CheckAliveInterval;



        /// <summary>
        /// Flag used to identify if a TCP object was disposed or not.
        /// it is obsolete.
        /// </summary>
        public bool isDisposed = false;



        /// <summary>
        /// Object used as semaphore to restrict access to a single thread
        /// at a time to avoid cross-threaded access violation.
        /// </summary>
        private Object _Locker = new Object();

        /// <summary>
        /// String used to store the name of the class that instanciated this
        /// class.
        /// </summary>
        public string Owner;

        #endregion

        #region EVENTS

        /// <summary>
        /// EventHandler used to declare events that will be raised when the server
        /// will receive a message from a connected client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void MessageReceivedEventHandler(object sender, MyTcpEventArgs e);



        /// <summary>
        /// EventHandler used to declare events that will be raised when a connected
        /// client is no longer connected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ClientDisconnectedEventHandler(object sender, MyTcpEventArgs e);



        /// <summary>
        /// EventHandler used to declare events that will be raised when a new client
        /// has connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void NewClientConnectedEventHandler(object sender, MyTcpEventArgs e);



        /// <summary>
        /// Event raised when a connected client sends a message to the server.
        /// </summary>
        public event MessageReceivedEventHandler OnMessageReceived;



        /// <summary>
        /// Event raised when a connected client has disconnected.
        /// </summary>
        public event ClientDisconnectedEventHandler OnClientDisconnected;



        /// <summary>
        /// Event raised when a new client has connected to the server.
        /// </summary>
        public event NewClientConnectedEventHandler OnNewClientConnected;



        /// <summary>
        /// Method called to raise a new client connected event
        /// </summary>
        /// <param name="client"></param>
        protected virtual void RaiseOnNewClientConnected(MyTcpClient client)
        {
            OnNewClientConnected?.Invoke(this, new MyTcpEventArgs() { Client = client });
        }



        /// <summary>
        /// Method subscribed to the OnMessageReceived event.
        /// it will be called when a new message is received from a connected
        /// client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnClientMessageReceived(object sender, MyTcpEventArgs e)
        {
            OnMessageReceived?.Invoke(this, e);
        }



        /// <summary>
        /// Method subscribed to the OnClientDisconnected event.
        /// it will be called when a connected client has disconnected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnClientDisconnect(object sender, MyTcpEventArgs e)
        {
            var client = (MyTcpClient)sender;
            //notify others what client is disconnected
            OnClientDisconnected?.Invoke(sender, new MyTcpEventArgs() { Client = e.Client });

            //Console.WriteLine("server:{0} has informed that it disconnected", client.client.Client.RemoteEndPoint.ToString());
            MyClients.Remove(client);
            client.client.Close();
            client.stream.Close();
            client.isDisposed = true;
            //client.client.Dispose();
            //client.stream.Dispose();
        }

        #endregion

        #region CONSTRUCTOR
        /// <summary>
        /// for xmltools only
        /// </summary>
        public MyTcpListener()
        {

        }
        /// <summary>
        /// This Constructor has by default disabled the check-if-alive for all connected clients 
        /// </summary>
        /// <param name="Ip">IP where the servers should open a socket</param>
        /// <param name="Port">Port opened for listening</param>
        public MyTcpListener(IPAddress Ip, int Port)
        {
            Init(Ip, Port, 1);
        }



        /// <summary>
        /// Constructor for a server
        /// </summary>
        /// <param name="Ip">IP where the servers should open a socket</param>
        /// <param name="Port">Port opened for listening</param>
        /// <param name="CheckAliveInterval">Interval for checking if connected clients are still alive</param>
        public MyTcpListener(IPAddress Ip, int Port, int CheckAliveInterval)
        {
            Init(Ip, Port, CheckAliveInterval);
        }



        /// <summary>
        /// This constructor also needs an event handler to call when receives a message
        /// </summary>
        /// <param name="Ip">IP where the servers should open a socket</param>
        /// <param name="Port">Port opened for listening</param>
        /// <param name="CheckAliveInterval">Interval for checking if connected clients are still alive</param>
        public MyTcpListener(IPAddress Ip, int Port, int CheckAliveInterval, MessageReceivedEventHandler Meh)
        {
            this.OnMessageReceived += Meh;
            Init(Ip, Port, CheckAliveInterval);
        }



        ~MyTcpListener()
        {
            foreach (var item in MyClients)
            {
                item.isDisposed = true;
                item.StopRequest = true;
                //item.client.Dispose();
                //item.stream.Dispose();
            }
            isDisposed = true;
            Close();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Method called in the constructor to initialize the
        /// server and the client list objects.
        /// </summary>
        /// <param name="Ip"></param>
        /// <param name="Port"></param>
        /// <param name="CheckAliveInterval"></param>
        private void Init(IPAddress Ip, int Port, int CheckAliveInterval)
        {
            Opened = false;
            this.CheckAliveInterval = CheckAliveInterval;
            stopRequest = false;
            Listener = new TcpListener(Ip, Port);
            MyClients = new List<MyTcpClient>();
        }



        /// <summary>
        /// Method called to start the hosting of the server. To open the
        /// socket and listen for connections and accept messages.
        /// </summary>
        public void Start()
        {
            Opened = true;
            if (thread != null && thread.IsAlive)
                thread.Abort();
            thread = new Thread(ListenerThread);
            thread.Name = "MyTcpListener ListenerThread " + Owner;
            Listener.Start();
            thread.Start();
        }



        /// <summary>
        /// Method called to send message to a connected client.
        /// </summary>
        /// <param name="client">MyTcpClient handler of a connected client</param>
        /// <param name="message">string message that will be sent</param>
        public void SendMessage(MyTcpClient client, string message)
        {
            if (!client.isDisposed)
            {

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
                NetworkStream stream = client.client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
        }

        public void SendMessage(MyTcpClient client, byte[] msg)
        {
            if (!client.isDisposed)
            {
                NetworkStream stream = client.client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
        }


        /// <summary>
        /// Method called to send a message to a connected client.
        /// </summary>
        /// <param name="client">TcpClient handler</param>
        /// <param name="message">string message that will be sent</param>
        public void SendMessage(TcpClient client, string message)
        {
            try
            {
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); ;
            }
        }



        /// <summary>
        /// Method called to send a message to a connected client.
        /// </summary>
        /// <param name="clientIndex">Index number of a client from a list of all clients connected.</param>
        /// <param name="message">string message that will be sent</param>
        public void SendMessage(int clientIndex, string message)
        {
            if (!isDisposed)
            {
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
                NetworkStream stream = MyClients[clientIndex].client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
        }


        public void SendMessage(int clientIndex, byte[] msg)
        {
            if (!isDisposed)
            {
                NetworkStream stream = MyClients[clientIndex].client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
        }

        /// <summary>
        /// Method called when the closing of the server is desired.
        /// it will close the socket and disconnect all the connected clients.
        /// </summary>
        public void Close()
        {

            StopRequest = true;
            Thread.Sleep(10);
            foreach (MyTcpClient client in MyClients)
            {
                client.StopRequest = true;
                //client.client.Dispose();
            }

            //Listener.Server.Disconnect(true);
            //Listener.Server.Dispose();
        }

        #endregion

        #region THREAD METHODS

        /// <summary>
        /// Method of the thread that will continue checking for new connections
        /// in a loop, every 200 ms.
        /// When the StopRequest bool flag will be true, it will exit the loop
        /// and the thread will be disposed.
        /// </summary>
        private void ListenerThread()
        {
            int aliveInterval;

            lock (_Locker)
            {
                aliveInterval = CheckAliveInterval;
            }

            while (!StopRequest)
            {
                CheckForNewClients(aliveInterval);
                Thread.Sleep(200);
            }
        }



        /// <summary>
        /// Method called in the ListenerThread for checking for new connections.
        /// When a client is connected, the handler to it will be stored in a client
        /// list, and it will be subscribed to the OnMessageReceived and OnDisconnect
        /// events.
        /// </summary>
        /// <param name="aliveInterval"></param>
        private void CheckForNewClients(int aliveInterval)
        {
            TcpListener _listener;

            lock (_Locker)
            {
                _listener = Listener;
            }
            try
            {
                if (_listener.Pending())
                {
                    lock (_Locker)
                    {
                        var client = new MyTcpClient(_listener.AcceptTcpClient(), aliveInterval);
                        client.Owner = this.Owner + " " + client;

                        //Application.Current.Dispatcher.Invoke(() => { client.OnMessageReceived += this.OnClientMessageReceived; client.OnDisconnect += this.OnClientDisconnect; });
                        //Application.Current.Dispatcher.Invoke(() => { MyClients.Add(client); });
                        //Application.Current.Dispatcher.Invoke(() => RaiseOnNewClientConnected(client));

                        client.OnMessageReceived += this.OnClientMessageReceived;
                        client.OnDisconnect += this.OnClientDisconnect;
                        RaiseOnNewClientConnected(client);
                        MyClients.Add(client);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion
    }

    /// <summary>
    /// MyTcpClient is a TCP library part of the MyFramework used as
    /// TCP client communication with another TCP server.
    /// </summary>
    public class MyTcpClient
    {
        #region PROPERTIES

        /// <summary>
        /// TcpClient object used to handle the client TCP communication.
        /// </summary>
        public TcpClient client;



        /// <summary>
        /// Thread used to check for incoming messages from the server.
        /// </summary>
        private Thread thread;



        /// <summary>
        /// Object used as semaphore to restrict access to a single thread
        /// at a time.
        /// </summary>
        private Object StopRequest_locker = new Object();



        /// <summary>
        /// StopRequest is a bool flag used to inform running threads
        /// to exit their loops and dispose.
        /// </summary>
        private bool stopRequest;



        /// <summary>
        /// StopRequest is a bool flag used to inform running threads
        /// to exit their loops and dispose.
        /// </summary>
        public bool StopRequest
        {
            get
            {
                lock (StopRequest_locker)
                {
                    return stopRequest;
                }
            }
            set
            {
                lock (StopRequest_locker)
                {
                    stopRequest = value;
                }
            }
        }



        /// <summary>
        /// NetworkStream object used to get/send messages to the server.
        /// </summary>
        public NetworkStream stream;



        /// <summary>
        /// Interval of time that the client will check if the server where
        /// is connected is still reachable.
        /// This is done by sending dummy data and checking if it could be
        /// succesfully sent.
        /// </summary>
        public int CheckAliveInterval = 0;



        /// <summary>
        /// Used to trigger the moment when the checkalive is performed.
        /// </summary>



        /// <summary>
        /// Flag used to identify if the tcp client object was disposed.
        /// </summary>
        public bool isDisposed = false;



        /// <summary>
        /// Object used as semaphore to restrict access to a single thread
        /// at a time to avoid cross-thread violations.
        /// </summary>
        public Object _locker = new Object();



        /// <summary>
        /// Name of the class that instantiated this object.
        /// </summary>
        public string Owner;

        #endregion

        #region EVENTS

        /// <summary>
        /// EventHandler used to declare events when a message is received
        /// from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void MessageReceived(object sender, MyTcpEventArgs e);



        /// <summary>
        /// OnMessageReceived event is raised when there is a message from the server.
        /// </summary>
        public event MessageReceived OnMessageReceived;



        /// <summary>
        /// Method called to rise the OnMessageReceived event.
        /// </summary>
        /// <param name="_msg"></param>
        protected virtual void RaiseOnMessageReceived(string _msg, byte[] _buffer)
        {
            OnMessageReceived?.Invoke(this, new MyTcpEventArgs() { Client = this, Message = _msg, Buffer=_buffer });
        }



        /// <summary>
        /// Method called when a tick event is raised from the timer object.
        /// This will launch a new thread that will check the connection 
        /// status with the server.
        /// The old (commented code) was performed on the main thread, causing
        /// the UI getting stuck at regular intervals. The thread purpouse is
        /// to avoid this issue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCheckAliveTimer_Tick(object sender, EventArgs e)
        {
            new Thread(() => checkAlive_thread()).Start();

            //if (StopRequest)
            //    checkAliveTimer.Stop();
            //else
            //{
            //    try
            //    {
            //        if (stream.CanWrite)
            //        {
            //            Console.WriteLine(client.Client.RemoteEndPoint.ToString() + ":checking if alive...");
            //            if (!isDisposed)
            //                stream.Write(new byte[] { (byte)'\0' }, 0, 1);
            //        }
            //        else
            //        {
            //            if (!isDisposed)
            //                Console.WriteLine(client.Client.RemoteEndPoint.ToString() + ":is disconnected, disposing object...");
            //            StopRequest = true;
            //            checkAliveTimer.Stop();
            //            RaiseDisconnectEvent();
            //        }
            //    }
            //    catch (Exception eex)
            //    {

            //        //Console.WriteLine(client.Client.RemoteEndPoint.ToString() + ":is disconnected, disposing object...");
            //        StopRequest = true;
            //        checkAliveTimer.Stop();
            //        RaiseDisconnectEvent();
            //    }
            //}
        }



        /// <summary>
        /// The method of the thread launched in the timer tick to check if the
        /// connection with the server is still online.
        /// </summary>
        private void checkAlive_thread()
        {
            lock (StopRequest_locker)
            {
                if (!StopRequest)
                {
                    try
                    {
                        if (stream.CanWrite)
                        {
                            Console.WriteLine(client.Client.RemoteEndPoint.ToString() + ":checking if alive...");
                            if (!isDisposed)
                                stream.Write(new byte[] { (byte)'\0' }, 0, 1);
                        }
                        else
                        {
                            if (!isDisposed)
                                Console.WriteLine(client.Client.RemoteEndPoint.ToString() + ":is disconnected, disposing object...");
                            StopRequest = true;
                            RaiseDisconnectEvent();
                        }
                    }
                    catch (Exception eex)
                    {

                        //Console.WriteLine(client.Client.RemoteEndPoint.ToString() + ":is disconnected, disposing object...");
                        StopRequest = true;
                        RaiseDisconnectEvent();
                    }
                }
            }
        }



        /// <summary>
        /// Event handler used to declare events that will be raised when the client
        /// disconnected from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void DisconnectedEventHandler(object sender, MyTcpEventArgs e);



        /// <summary>
        /// Disconnected event raised when the client is disconnected from the server.
        /// </summary>
        public event DisconnectedEventHandler OnDisconnect;



        /// <summary>
        /// Method called to raise the OnDisconnect event
        /// </summary>
        protected virtual void RaiseDisconnectEvent()
        {
            OnDisconnect?.Invoke(this, new MyTcpEventArgs() { Client = this });
        }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor used by the server.
        /// To define a MyTcpClient manualy, use the other constructors.
        /// to initialize a MyTcpClient.
        /// </summary>
        /// <param name="_client"></param>
        /// <param name="CheckAliveInterval"></param>
        public MyTcpClient(TcpClient _client, int CheckAliveInterval)
        {
            Init(_client, CheckAliveInterval);
            Start();
        }



        /// <summary>
        /// default constructor, by default the check for alive connection is disabled
        /// </summary>
        public MyTcpClient()
        {
            client = new TcpClient();
        }



        /// <summary>
        /// Constructor with defined alive-check-interval
        /// </summary>
        /// <param name="_aliveInterval">the interval passed between check for connection</param>
        public MyTcpClient(int _aliveInterval)
        {
            CheckAliveInterval = _aliveInterval;
            client = new TcpClient();
        }



        /// <summary>
        /// Method called to connect to a specified server.
        /// </summary>
        /// <param name="ip">ip of the server</param>
        /// <param name="port">port of the server</param>
        public void Connect(IPAddress ip, int port)
        {
            try
            {
                if (!isDisposed)
                {
                    client.Connect(ip, port);
                    Init(client, CheckAliveInterval);
                    Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        ~MyTcpClient()
        {
            //client.Dispose();
            //stream.Dispose();
            try
            {
                isDisposed = true;
                StopRequest = true;
                client.Close();
                stream.Close();
                //thread.Abort();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Method called to start the listening for server messages.
        /// </summary>
        private void Start()
        {
            thread.Start();
            //if (CheckAliveInterval != 0)
            //checkAliveTimer.Start();
        }



        /// <summary>
        /// Method called in the constructor to initialize global variables
        /// </summary>
        private void Init(TcpClient _client, int CheckAliveInterval)
        {
            this.CheckAliveInterval = CheckAliveInterval;
            //if (CheckAliveInterval != 0)
            //checkAliveTimer = new DispatcherTimer(new TimeSpan(CheckAliveInterval * 10000), DispatcherPriority.Normal, new EventHandler(OnCheckAliveTimer_Tick), Application.Current.Dispatcher);
            //checkAliveTimer.Interval = new TimeSpan(CheckAliveInterval);
            //checkAliveTimer.Tick += new EventHandler(OnCheckAliveTimer_Tick);
            thread = new Thread(ClientListenerThread);
            thread.Name = "MyTcpClient ClientListenerThread " + Owner;
            StopRequest = false;
            client = _client;
            stream = client.GetStream();
        }



        /// <summary>
        /// Method called to send a message to the server.
        /// </summary>
        /// <param name="message">string message that will be sent to the server</param>
        public void SendMessage(string message)
        {
            try
            {
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(message);
                NetworkStream stream = client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public void SendMessage(byte[] msg)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region THREAD METHODS

        /// <summary>
        /// Method of the thread launched to check for server transmitions.
        /// </summary>
        private void ClientListenerThread()
        {
            while (!StopRequest)
            {
                CheckForData();
            }
        }

        /// <summary>
        /// Method called in the ClientListenerThread method to check for
        /// available data transmited from the server.
        /// </summary>
        private void CheckForData()
        {
            try
            {
                Byte[] bytes = new Byte[256];
                String data = null;
                bool _isDisposed;
                lock (_locker)
                {
                    if (!client.Connected)
                        StopRequest = true;
                    _isDisposed = isDisposed; ;

                    if (!_isDisposed)
                        while (stream.DataAvailable)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, stream.Read(bytes, 0, bytes.Length));
                            if (bytes[0] != 0)
                                RaiseOnMessageReceived(data,bytes);
                        }
                }
                Thread.Sleep(10);
            }
            catch (Exception eex)
            {
                //Console.WriteLine(eex.Message); ;
            }
        }

        public void Connect(object ipAddress)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

namespace R0013.Shared.SuperSocket
{

    public class SuperClient : ICommSocket
    {
        #region -- PROPERTIES --
        #region -- PUBLIC --
        public event OnMessageReceivedEventHandler OnMessageReceived;

        #endregion
        #region -- PRIVATE --
        private IEventAggregator _eventAggregator { get; set; }
        private MyTcpClient _client;
        private bool _stopRequest { get; set; } = false;
        #endregion
        #endregion

        #region -- CONSTRUCTOR --
        public SuperClient(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _client = new MyTcpClient();
            var ip = IPAddress.Parse(MqttCommSettings.ServerAddress);
            var port = MqttCommSettings.ServerPort;

            _client.Connect(ip, port);
            _client.OnMessageReceived += _client_OnMessageReceived;
            //_clientInit();
            //Start();
            //new Thread(_keepAliveThread).Start();
        }

        private void _client_OnMessageReceived(object sender, MyTcpEventArgs e)
        {
            OnMessageReceived?.Invoke(e.Buffer);
        }
        #endregion

        #region -- FUNCTIONS --
        #region -- PUBLIC --
        public void Send(byte[] obj)
        {
            _client.SendMessage(obj);
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }
        #endregion
        #region -- PRIVATE --






        #endregion
        #endregion
    }

    public class SuperServer : ICommSocket
    {
        #region -- PROPERTIES --
        #region -- PUBLIC --
        public event OnMessageReceivedEventHandler OnMessageReceived;
        #endregion
        #region -- PRIVATE --
        private MyTcpListener _server;
        private IEventAggregator _eventAggregator { get; set; }
        #endregion
        #endregion

        #region -- CONSTRUCTOR --
        public SuperServer(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            //_serverInit();
            var ip = IPAddress.Parse(MqttCommSettings.ServerAddress);
            var port = MqttCommSettings.ServerPort;
            _server = new MyTcpListener(ip, port);
            _server.OnMessageReceived += _server_OnMessageReceived;
            _server.Start();
        }

        private void _server_OnMessageReceived(object sender, MyTcpEventArgs e)
        {
            OnMessageReceived?.Invoke(e.Buffer);
        }
        #endregion

        #region -- FUNCTIONS --
        #region -- PUBLIC --

        public void Send(byte[] obj)
        {
            _server.SendMessage(0, obj);
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
        #endregion
        #region -- PRIVATE -- 
        private void _serverInit()
        {
            var ip = IPAddress.Parse(MqttCommSettings.ServerAddress);
            var port = MqttCommSettings.ServerPort;
            _server = new MyTcpListener(ip, port);
            _server.OnMessageReceived += _server_OnMessageReceived1; ;
        }

        private void _server_OnMessageReceived1(object sender, MyTcpEventArgs e)
        {
            OnMessageReceived?.Invoke(e.Buffer);
        }

        #endregion
        #endregion
    }
}