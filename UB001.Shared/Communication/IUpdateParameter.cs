using System;
using System.Reflection;

namespace R0013.Shared.Communication
{
    public interface IUpdateParameter
    {
        string Value { get; set; }
        byte[] Data { get; set; }
        string ParameterName { get; set; }
        string GetValue();
        byte[] GetData();
        Func<string> ParameterValueGetter { get; set; }
        Func<byte[]> ParameterDataGetter { get; set; }
        Func<bool> CanUpdateCondition { get; set; }
        int UpdateInterval_ms { get; set; }
        DateTime LastUpdated { get; set; }
        Action<string,byte[]> ExecuteOnMessageReceived { get; set; }
        Action ExecuteOnFailure { get; set; }
        PropertyInfo Property { get; set; }
        void ExecuteOnSuccess();
        bool ShouldUpdate();
        IUpdateParameter SetExecutionOnMessageReceived(Action<string,byte[]> ua);
        IUpdateParameter SetExecutionOnFailure(Action ta);
        IUpdateParameter SetUpdateInterval(int ui_ms);
        IUpdateParameter SetCanUpdateCondition(Func<bool> cu);
        IUpdateParameter SetParameterValueGetter(Func<string> valueGetter);
        IUpdateParameter SetParameterDataGetter(Func<byte[]> dataGetter);
        bool KeepInBufferUntilTimeout { get; set; }
    }
}
