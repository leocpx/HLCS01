using ImpromptuInterface;
using Prism.Events;
using R0013.ServiceCore.Attributes;
using R0013.ServiceCore.Models;
using R0013.ServiceCore.PubSubEvents;
using R0013.Shared.Attributes;
using R0013.Shared.Communication;
using R0013.Shared.PubSubEvents;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace R0013.ServiceCore.Bases
{
    public abstract class MessageConsumerBase
    {
        #region -- PROPERTIES --

        #region -- PUBLIC --
        public IEventAggregator _eventAggregator { get; set; }
        #endregion

        #region -- PRIVATE --
        private List<MessageConsumerInstruction> MessageInstructions { get; set; }
        private SubscriptionToken _subscriptionToken { get; set; }
        private IUnivSerializer _univSerializer { get; set; }
        #endregion

        #endregion

        #region -- CONSTRUCTOR --
        public MessageConsumerBase(IEventAggregator eventAggregator, IUnivSerializer univSerializer)
        {
            _eventAggregator = eventAggregator;
            _univSerializer = univSerializer;
            _subscriptionToken = _eventAggregator.GetEvent<OnMessageReceivedEvent>().Subscribe(MessageConsumer, true);
            _eventAggregator.GetEvent<OnCloseEvent>().Subscribe(_subscriptionToken.Dispose);

            RegisterMessageConsumers();
        }
        #endregion

        #region -- FUNCTIONS --
        #region -- PUBLIC --
        public void SendEvent(string eventName, object eventData)
        {
            var _sT = typeof(CommMessage).GetField(eventName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var st = (EventDataTypeAttribute)_sT.GetCustomAttribute(typeof(EventDataTypeAttribute));
            var _serializeType = st.DataType;

            var serializeMethod = _univSerializer.GetType().GetMethod("_Serialize").MakeGenericMethod(_serializeType);
            var serializedData = serializeMethod?.Invoke(_univSerializer, new object[] { eventData });

            var eventMessage = new
            {
                Id = -1,
                Value = "",
                Data = serializedData,
                ParameterName = eventName
            }.ActLike<IMessage>();

            _eventAggregator.GetEvent<OnForwardMessageRequestEvent>().Publish(eventMessage);
        }
        #endregion
        #region -- PRIVATE --
        private void RegisterMessageConsumers()
        {
            MessageInstructions = new List<MessageConsumerInstruction>();
            var consumerMethods = GetType().GetMethods().Where(_m => _m.GetCustomAttributes(typeof(MessageConsumerAttribute), true).Any()).ToList();

            foreach (var consumer in consumerMethods)
            {
                var _attribute = consumer.GetCustomAttributes(typeof(MessageConsumerAttribute), true).First() as MessageConsumerAttribute;
                var _messageConsumer = new MessageConsumerInstruction()
                {
                    ParameterName = _attribute.CommandName,
                    ResponseRequired = _attribute.ResponseRequired,
                    ProcessMessage = (v, d) => consumer.Invoke(this, new object[] { v, d })
                };

                MessageInstructions.Add(_messageConsumer);
            }
        }

        private void MessageConsumer(IMessage message)
        {
            var foundInstruction = MessageInstructions.Where(_m => _m.ParameterName == message.ParameterName).FirstOrDefault();
            if (foundInstruction == null) return;
            var executionResult = foundInstruction.ProcessMessage(message.Value, message.Data);

            if (foundInstruction.ResponseRequired)
            {
                var processedMessage = executionResult != null ? GetProcessedMessage(executionResult, message) : message;

                _eventAggregator.GetEvent<OnForwardMessageRequestEvent>().Publish(processedMessage);
            }
        }

        private IMessage GetProcessedMessage(object executionResult, IMessage message)
        {
            IMessage processedMessage = null;

            if (executionResult.GetType() == typeof(string))
                processedMessage = new
                {
                    Id = message.Id,
                    Value = (string)executionResult,
                    Data = message.Data,
                    ParameterName = message.ParameterName
                }.ActLike<IMessage>();

            if (executionResult.GetType() == typeof(byte[]))
                processedMessage = new
                {
                    Id = message.Id,
                    Value = message.ParameterName,
                    Data = (byte[])executionResult,
                    ParameterName = message.ParameterName
                }.ActLike<IMessage>();

            return processedMessage;
        }
        #endregion

        #endregion
    }


    //public class PressureControllerService
    //{
    //    #region -- PROPERTIES --
    //    #region -- PRIVATE --
    //    private SubscriptionToken _subscriptionToken { get; set; }
    //    private List<MessageConsumerInstruction> MessageInstructions { get; set; }
    //    private IEventAggregator _eventAggregator { get; set; }
    //    #endregion
    //    #endregion

    //    #region -- CONSTRUCTOR --
    //    public PressureControllerService(IEventAggregator eventAggregator)
    //    {
    //        _eventAggregator = eventAggregator;

    //        MessageInstructions = GetMessageInstructions();
    //        _subscriptionToken = _eventAggregator.GetEvent<OnMessageReceivedEvent>().Subscribe(MessageConsumer, true);
    //        _eventAggregator.GetEvent<OnCloseEvent>().Subscribe(_subscriptionToken.Dispose);
    //    }
    //    #endregion

    //    #region -- FUNCTIONS --
    //    #region -- PRIVATE --
    //    private void MessageConsumer(IMessage message)
    //    {
    //        var foundInstruction = MessageInstructions.Where(_m => _m.ParameterName == message.ParameterName).FirstOrDefault();

    //        if (foundInstruction != null)
    //        {
    //            var executionResult = foundInstruction.ProcessMessageInstruction(message.Value);
    //            var processedMessage = new { Id = message.Id, Value = executionResult, ParameterName = message.ParameterName }.ActLike<IMessage>();
    //            _eventAggregator.GetEvent<OnForwardMessageRequestEvent>().Publish(processedMessage);
    //        }
    //    }
    //    private List<MessageConsumerInstruction> GetMessageInstructions()
    //    {
    //        return new List<MessageConsumerInstruction>()
    //        {
    //            new MessageConsumerInstruction() { ParameterName = "PressureController_GetPV", ProcessMessageInstruction = (p)=> Respond(1)},
    //            new MessageConsumerInstruction() { ParameterName = "PressureController_GetSP", ProcessMessageInstruction = (p)=> Respond(2)},
    //            new MessageConsumerInstruction() { ParameterName = "PressureController_SetPV", ProcessMessageInstruction = (p)=> $"set {p} - DONE"},
    //            new MessageConsumerInstruction() { ParameterName = "PressureController_SetSP", ProcessMessageInstruction = (p)=> $"set {p} - FAIL"},
    //            new MessageConsumerInstruction() { ParameterName = "SliderController_PV", ProcessMessageInstruction = (p) => ManageSlider(p) }
    //        };
    //    }
    //    private string _sliderValue = "";

    //    private string ManageSlider(string v)
    //    {
    //        double result = 0;

    //        if(double.TryParse(v, out result))
    //        {
    //            return result.ToString();
    //        }
    //        return "";
    //    }
    //    private string Respond(int i)
    //    {
    //        switch (i)
    //        {
    //            case 1:
    //                return (DateTime.Now.Millisecond/10.0 - 35.463).ToString();
    //            case 2:
    //                return (DateTime.Now.Millisecond/10.0 - 26.463).ToString();
    //            case 3:
    //                return (DateTime.Now.Millisecond).ToString();
    //            default:
    //                return "";
    //        }
    //    }
    //    #endregion
    //    #endregion
    //}
}
