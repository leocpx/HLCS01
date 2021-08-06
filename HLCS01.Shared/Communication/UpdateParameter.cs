using System;
using System.Reflection;

namespace HLCS01.Shared.Communication
{
    public class UpdateParameter : IUpdateParameter
    {
        public string Value { get; set; }
        public byte[] Data { get; set; }
        public string ParameterName { get; set; }
        public Func<bool> CanUpdateCondition { get; set; }
        public Func<string> ParameterValueGetter { get; set; }
        public Func<byte[]> ParameterDataGetter { get; set; }
        public int UpdateInterval_ms { get; set; }
        public DateTime LastUpdated { get; set; }
        public Action<string,byte[]> ExecuteOnMessageReceived { get; set; }
        public Action ExecuteOnFailure { get; set; }
        public PropertyInfo Property { get; set; }
        public bool KeepInBufferUntilTimeout { get; set; } = false;
        public UpdateParameter(string name, PropertyInfo property)
        {
            this.Property = property;
            this.ParameterName = name;
            LastUpdated = DateTime.Now;
            CanUpdateCondition = () => true;
            ParameterValueGetter = () => "";
        }

        public UpdateParameter(string name)
        {
            this.ParameterName = name;
            LastUpdated = DateTime.Now;
            CanUpdateCondition = () => true;
            ParameterValueGetter = () => "";
        }

        public IUpdateParameter SetUpdateInterval(int ui_ms)
        {
            UpdateInterval_ms = ui_ms;
            return this;
        }
        public IUpdateParameter SetExecutionOnMessageReceived(Action<string,byte[]> ua)
        {
            ExecuteOnMessageReceived = ua;
            return this;
        }
        public IUpdateParameter SetExecutionOnFailure(Action ta)
        {
            ExecuteOnFailure = ta;
            return this;
        }
        public IUpdateParameter SetParameterValueGetter(Func<string> valueGetter)
        {
            ParameterValueGetter = valueGetter;
            return this;
        }
        public IUpdateParameter SetParameterDataGetter(Func<byte[]> dataGetter)
        {
            ParameterDataGetter = dataGetter;
            return this;
        }
        public void ExecuteOnSuccess()
        {
            ExecuteOnMessageReceived?.Invoke(Value,Data);
        }
        public IUpdateParameter SetCanUpdateCondition(Func<bool> cu)
        {
            CanUpdateCondition = cu;
            return this;
        }

        public string GetValue()
        {
            return ParameterValueGetter?.Invoke();
        }

        public byte[] GetData()
        {
            return ParameterDataGetter?.Invoke();
        }

        public bool ShouldUpdate()
        {
            if (CanUpdateCondition == null) return false;
            if(CanUpdateCondition())
            {
                var elapsed = (DateTime.Now - LastUpdated).TotalMilliseconds;
                if (elapsed >= UpdateInterval_ms)
                {
                    LastUpdated = DateTime.Now;
                    return true;
                }
            }
            return false;
        }
    }
}
