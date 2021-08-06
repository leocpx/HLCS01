using System;

namespace R0013.ServiceCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageConsumerAttribute : Attribute
    {
        public string CommandName { get; set; }
        public bool ResponseRequired { get; set; }
        public MessageConsumerAttribute(string command, bool response)
        {
            CommandName = command;
            ResponseRequired = response;
        }
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
