using Prism.Mvvm;
using HLCS01.Shared.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HLCS01.Shared.Communication
{
    public interface IDataProvider
    {
        void DoDataProviderStuff();
        string GetProvidedText();
        List<IUpdateParameter> ParametersUpdateList { get; set; }
        void RegisterParameter(IUpdateParameter _updateParameter);
        void AutoRegisterAllDefaultMonitors(MonitorBase viewModel);
        void AutoRegisterAllDefaultEventMonitors(MonitorBase viewModel);
        void WriteRemoteParameter(IUpdateParameter _updateParameter, int timeout);
        IUpdateParameter GetUpdateParameter(PropertyInfo property);
        void OverrideParameterUpdateCondition(PropertyInfo property, Func<bool> _canUpdateCondition);
        void OverrideParameterValueInitializer(PropertyInfo property, Func<string> valueGetter);
        void OverrideParameterDataInitializer(PropertyInfo property, Func<byte[]> dataGetter);
    }
}
