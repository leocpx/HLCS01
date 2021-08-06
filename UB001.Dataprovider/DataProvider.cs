using Prism.Events;
using Prism.Mvvm;
using R0013.Shared.Communication;
using R0013.Shared.Attributes;
using R0013_HMI.Core.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using R0013.Shared.Models;

namespace R0013.DataProvider
{
    public class DataProvider : IDataProvider, IDisposable
    {
        #region -- PROPERTIES --

        #region -- PUBLIC --
        public List<IUpdateParameter> ParametersUpdateList { get; set; }
        #endregion

        #region -- PRIVATE --
        private ICommSocket _commClient { get; set; }
        private IEventAggregator _ea;
        private IUnivSerializer _univSerializer { get; set; }
        private bool _stopRequest { get; set; } = false;
        private MessageManager _messageManager;
        #endregion

        #endregion

        #region -- CONSTRUCTOR --

        public DataProvider(ICommSocket commClient, IEventAggregator ea, ICommSerializer _commSerializer, IUnivSerializer univSerializer)
        {
            _ea = ea;
            _commClient = commClient;
            _univSerializer = univSerializer;
            _messageManager = new MessageManager(_ea, _commClient, _commSerializer);
            ParametersUpdateList = new List<IUpdateParameter>();
            _ea.GetEvent<OnClosingEvent>().Subscribe(
                () =>
                {
                    _stopRequest = true;
                    _commClient.Stop();
                });

            new Thread(UpdateThread).Start();
        }

        #endregion

        #region -- FUNCTIONS --

        #region -- PUBLIC --
        public void DoDataProviderStuff()
        {
        }

        public string GetProvidedText()
        {
            return DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
        }

        public void Dispose()
        {
            _stopRequest = true;
        }
        private void RegisterEvent(IUpdateParameter updateParameter)
        {
            _messageManager.RegisterEvent(updateParameter);
        }
        public void RegisterParameter(IUpdateParameter _updateParameter)
        {
            ParametersUpdateList.Add(_updateParameter);
        }
        public IUpdateParameter GetUpdateParameter(PropertyInfo property)
        {
            return ParametersUpdateList.FirstOrDefault(_p => _p.Property == property);
        }
        public void OverrideParameterValueInitializer(PropertyInfo property, Func<string> valueGetter)
        {
            var updateParam = GetUpdateParameter(property);
            updateParam.SetParameterValueGetter(valueGetter);
        }
        public void OverrideParameterDataInitializer(PropertyInfo property, Func<byte[]> dataGetter)
        {
            var updateParam = GetUpdateParameter(property);
            updateParam.SetParameterDataGetter(dataGetter);
        }
        public void OverrideParameterUpdateCondition(PropertyInfo property, Func<bool> _canUpdateCondition)
        {
            var updateParam1 = GetUpdateParameter(property);
            updateParam1.SetCanUpdateCondition(_canUpdateCondition);
        }

        public void AutoRegisterAllDefaultEventMonitors(MonitorBase viewModel)
        {
            var properties = viewModel.GetType().GetProperties();
            var eventMonitorProps = properties.Where(_p => _p.GetCustomAttribute(typeof(EventMonitorAttribute)) != null).ToList();

            eventMonitorProps.ForEach(
                emp =>
                {
                    var _attribute = (EventMonitorAttribute)emp.GetCustomAttribute(typeof(EventMonitorAttribute));
                    var _eventName = _attribute.EventName;
                    //var _serializeType = _attribute.SerializedType;
                    var _sT = typeof(CommMessage).GetField(_eventName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    var st= (EventDataTypeAttribute)_sT.GetCustomAttribute(typeof(EventDataTypeAttribute));
                    var _serializeType = st.DataType;

                    var newEventParameter = new UpdateParameter(_eventName)
                    .SetExecutionOnMessageReceived(
                        (v,d)=>
                        {
                            var deSerializer =_univSerializer.GetType().GetMethod("_Deserialize").MakeGenericMethod(_serializeType);
                            var data = deSerializer?.Invoke(_univSerializer, new object[] { d });
                            emp.SetValue(viewModel, data);
                        });

                    RegisterEvent(newEventParameter);
                   
                });
        }
        public void AutoRegisterAllDefaultMonitors(MonitorBase viewModel)
        {
            var properties = viewModel.GetType().GetProperties();
            var monitoredProperties = properties.Where(_p => _p.GetCustomAttribute(typeof(DefaultMonitorAttribute)) != null).ToList();

            monitoredProperties.ForEach(mp =>
            {
                var _attribute = (DefaultMonitorAttribute)mp.GetCustomAttribute(typeof(DefaultMonitorAttribute));
                var paramAddress = _attribute.ParameterName;
                var _updateInterva = _attribute.UpdateInterval;
                var updateText = new UpdateParameter(paramAddress, mp)
                    .SetUpdateInterval(_updateInterva)
                    .SetCanUpdateCondition(() => true)
                    .SetExecutionOnMessageReceived((val,dat) =>
                    {
                        if(mp.PropertyType==typeof(string))
                            mp.SetValue(viewModel, val);

                        if (mp.PropertyType == typeof(byte[]))
                            mp.SetValue(viewModel, dat);

                        //typeof(BindableBase).GetMethod("RaisePropertyChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(viewModel, new object[] { mp.Name });
                    });
                RegisterParameter(updateText);
            });
        }
        #endregion

        #region -- PRIVATE --
        private void UpdateThread()
        {
            while (!_stopRequest)
            {
                try
                {
                    var updateList = ParametersUpdateList.Where(parameter => parameter.ShouldUpdate()).ToList();
                    updateList.ForEach(parameter => _messageManager.RegisterRequest(parameter));
                }
                catch (Exception) { }
                Thread.Sleep(10);
            }
        }

        public void WriteRemoteParameter(IUpdateParameter _updateParameter, int timeout)
        {
            new Thread(() =>
            {
                // make sure the MessageManager doesn't remove it from the buffer
                _updateParameter.KeepInBufferUntilTimeout = true;
                var requestId = _messageManager.RegisterRequest(_updateParameter);
                var started = DateTime.Now;
                var elapsed = DateTime.Now - started;

                // wait untill timeout occurs 
                // or the request was removed from the messagerequestbuffer 
                // in this case means it was managed by the messagemanager (lol)
                while (elapsed.TotalMilliseconds < timeout || !_messageManager.MessageRequestBuffer.ContainsKey(requestId))
                {
                    elapsed = DateTime.Now - started;
                    Thread.Sleep(100);
                }

                // if after timeout our request is still pressent in the buffer, we call the timeoutaction method
                // set by the viewmodel (requester) and remove the request from the buffer list
                // this request will be kept in the messagerequestbuffer due to the KeepInBufferUntilTimeout flag
                // other requests will be automatically removed in FIFO manner if responses are not provided by the server
                if (_messageManager.MessageRequestBuffer.ContainsKey(requestId))
                {
                    _updateParameter.ExecuteOnFailure?.Invoke();
                    Message dumpedRequest;
                    _messageManager.MessageRequestBuffer.TryRemove(requestId, out dumpedRequest);
                }
            }).Start();
        }

    


        #endregion

        #endregion
    }
}

namespace R0013_HMI.Core.Events
{
    public class OnClosingEvent : PubSubEvent { }
}
