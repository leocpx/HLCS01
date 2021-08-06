using ImpromptuInterface;
using Prism.Events;
using R0013.Shared.Communication;
using R0013.Shared.Serializer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace R0013.DataProvider
{
    internal class MessageManager
    {
        #region -- PROPERTIES --
        public ConcurrentDictionary<int, Message> MessageRequestBuffer;
        public ConcurrentDictionary<int, Message> SubscribedEventsList;
        private static int id;
        private static int MaxBufferLength { get; set; } = 1000;
        private IEventAggregator _ea;
        private ICommSocket _comClient;
        private readonly ICommSerializer _serializer;

        #endregion

        #region -- CONSTRUCTOR --
        public MessageManager(IEventAggregator ea, ICommSocket comClient, ICommSerializer _commSerializer)
        {
            _comClient = comClient;
            _serializer = _commSerializer;
            _comClient.OnMessageReceived += OnMessageReceived;
            MessageRequestBuffer = new ConcurrentDictionary<int, Message>();
            SubscribedEventsList = new ConcurrentDictionary<int, Message>();
            _ea = ea;
            _ea.GetEvent<OnMessageResponseEvent>().Subscribe(
                m =>
                {
                    Message foundMessage;

                    // check if this received message was requested - if so remove it from the request buffer
                    if (!MessageRequestBuffer.TryRemove(m.Id, out foundMessage))
                    {
                        // if wasn't requested, check if its an event bassed message provided by the server
                        foundMessage = SubscribedEventsList.Values.FirstOrDefault(_e => _e.ParameterName == m.ParameterName);
                    
                        // if it's not found in the subscribed event list, this message can't be processed
                        if (foundMessage == null)
                            return;
                    }

                    foundMessage.UiParameter.Value = m.Value;
                    foundMessage.UiParameter.Data = m.Data;
                    foundMessage.UiParameter.ExecuteOnSuccess();
                });
        }
        #endregion

        #region -- FUNCTIONS --
        #region -- PUBLIC --
        public void RegisterEvent(IUpdateParameter updateParameter)
        {
            // all events are registered with -1 id so won't be confused with other ids in the requestbuffer
            // also the server will send the events with id -1
            SubscribedEventsList.TryAdd(-1, new Message(-1)
            {
                UiParameter = updateParameter,
            });
        }
        public int RegisterRequest(IUpdateParameter updateParameter)
        {
            // first check the max length of the buffer queue is not exceeded
            // remove the oldest request unless is based on a timeout
            if (MessageRequestBuffer.Count > MaxBufferLength)
            {
                var last = MessageRequestBuffer.Min(c => c.Key);
                Message lastMsg;
                MessageRequestBuffer.TryRemove(last, out lastMsg);
                
                
                // if this message has KeepInBufferUntilTimeout we
                // readd it to the buffer because it needs to be triggered
                // by the timeout event
                if (!lastMsg.UiParameter.KeepInBufferUntilTimeout)
                {
                    lastMsg.UiParameter.ExecuteOnFailure?.Invoke();
                }
                else
                {
                    var retryCount = 10;
                    var tries = 0;
                    var added = false;
                    // make sure we add it to the list
                    while (!added)
                    {
                        added=MessageRequestBuffer.TryAdd(last, lastMsg);
                        if (!added)
                        {
                            tries++;
                            Thread.Sleep(10);
                        }
                    }
                }
            }
            var newMessage = new Message(++id) { UiParameter = updateParameter, Value = updateParameter.GetValue(), Data = updateParameter.GetData() };
            MessageRequestBuffer.TryAdd(id, newMessage);
            var serialized = _serializer.Serialize(newMessage);
            _comClient.Send(serialized);
            return newMessage.Id;
        }

        #endregion
        #region -- PRIVATE --


        private void OnMessageReceived(byte[] value)
        {
            var transportMessage = _serializer.Deserialize(value);
            var message = new Message(transportMessage.Id) { Value = transportMessage.Value, Data = transportMessage.Data, ParameterName = transportMessage.ParameterName };
            _ea.GetEvent<OnMessageResponseEvent>().Publish(message);
        }  
        #endregion
        #endregion
    }
}
