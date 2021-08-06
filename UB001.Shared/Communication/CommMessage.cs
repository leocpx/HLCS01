using R0013.Shared.Attributes;
using R0013.Shared.Serializer.AlarmsModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R0013.Shared.Communication
{
    public static class CommMessage
    {
        /// <summary>
        /// PRESSURE CONTROLLER COMMANDS
        /// </summary>
        // -------------------------------------------------------------------------------
        public const string GET_PRESSURE_CONTROLLER_PV  = "GET_PRESSURE_CONTROLLER_PV";
        public const string GET_PRESSURE_CONTROLLER_SP  = "GET_PRESSURE_CONTROLLER_SP";
        public const string SET_PRESSURE_CONTROLLER_PV = "SET_PRESSURE_CONTROLLER_PV";
        public const string SET_PRESSURE_CONTROLLER_SP = "SET_PRESSURE_CONTROLLER_SP";
        // -------------------------------------------------------------------------------



        /// <summary>
        /// TEMPERATURE CONTROLLER COMMANDS
        /// </summary>
        // -------------------------------------------------------------------------------
        public const string GET_TEMPERATURE_CONTROLLER_PV  = "GET_TEMPERATURE_CONTROLLER_PV";
        public const string GET_TEMPERATURE_CONTROLLER_SP  = "GET_TEMPERATURE_CONTROLLER_SP";
        public const string SET_TEMPERATURE_CONTROLLER_PV = "SET_TEMPERATURE_CONTROLLER_PV";
        public const string SET_TEMPERATURE_CONTROLLER_SP = "SET_TEMPERATURE_CONTROLLER_SP";
        // -------------------------------------------------------------------------------


        /// <summary>
        /// HUMIDITY CONTROLLER COMMANDS
        /// </summary>
        // -------------------------------------------------------------------------------
        public const string SET_HUMIDITY_CONTROLLER_PV = "SET_HUMIDITY_CONTROLLER_PV";
        public const string GET_HUMIDITY_CONTROLLER_PV = "GET_HUMIDITY_CONTROLLER_PV;";
        // -------------------------------------------------------------------------------


        /// <summary>
        /// OTHER MISC. COMMANDS
        /// </summary>
        // -------------------------------------------------------------------------------
        public const string GET_SLIDER_CONTROLLER_PV  = "GET_SLIDER_CONTROLLER_PV";
        public const string SET_SLIDER_VALUE = "SET_SLIDER_VALUE";
        public const string GET_DATA = "GET_DATA";
        public const string GET_TEXT = nameof(GET_TEXT);
        // -------------------------------------------------------------------------------


        /// <summary>
        /// EVENT BASED MESSAGES
        /// USED WITH EventMonitorAttribute to subscribe viewmodel properties to event based 
        /// updates (no request message is required)
        /// 
        /// must contain string value of its name, reflection needs to retrieve EventDataType 
        /// attribute
        /// </summary>
        // -------------------------------------------------------------------------------
        [EventDataType(typeof(AlarmsContainer))]
        public const string ALARMS_EVENT = nameof(ALARMS_EVENT);

        [EventDataType(typeof(DataContainer))]
        public const string GET_DATE_CONTAINER = nameof(GET_DATE_CONTAINER);
        // -------------------------------------------------------------------------------
    }

    [MessagePack.MessagePackObject]
    public class DataContainer
    {
        [MessagePack.Key(0)]
        public DateTime Data { get; set; }
    }
}
