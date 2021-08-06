using Prism.Events;
using Prism.Mvvm;
using HLCS01.Shared.Attributes;
using HLCS01.Shared.Communication;
using HLCS01.Shared.PubSubEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HLCS01.Shared.Models
{
    public abstract class MonitorBase
    {
        public abstract int TabIndex { get; set; }
        private bool _tabIsSelected { get; set; }

        public MonitorBase(IDataProvider _dataProvider, IEventAggregator _eventAggregator)
        {
            _dataProvider.AutoRegisterAllDefaultMonitors(this);
            _dataProvider.AutoRegisterAllDefaultEventMonitors(this);

            PropertiesToOverride().ForEach(
                p => _dataProvider.OverrideParameterUpdateCondition(p, () => _tabIsSelected));

            _eventAggregator.GetEvent<OnTabSelectedEvent>().Subscribe(
                _t =>
                {
                    _tabIsSelected = TabIndex == _t;
                });
        }

        private List<PropertyInfo> PropertiesToOverride()
        {
            try
            {
                var properties = GetType().GetProperties()
            .ToList()
            .Where(p => p.GetCustomAttributes((typeof(UpdateOnlyWhenVisibleAttribute))).Any())
            .ToList();

                return properties;
            }
            catch (Exception) { }
            return new List<PropertyInfo>();
        }
    }
}
