using HLCS01.Shared.Communication;

namespace HLCS01.DataProvider
{
    internal class Message : IMessage
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public byte[] Data { get; set; }
        public string ParameterName
        {
            get
            {
                if(UiParameter!=null)
                    return UiParameter.ParameterName;

                return _parameterName;
            }
            set
            {
                if (UiParameter != null)
                    UiParameter.ParameterName = value;
                else
                    _parameterName = value;
            }
        }
        private string _parameterName = "";
        public IUpdateParameter UiParameter;
        public Message(int id)
        {
            Id = id;
        }
    }
}
