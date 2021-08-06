using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLCS01.Shared.Serializer.AlarmsModel
{
    [MessagePack.MessagePackObject]
    public class AlarmsContainer
    {
        [Key(0)]
        public List<Alarm> AlarmCollection { get; set; } = new List<Alarm>();
    }

    [MessagePack.MessagePackObject]
    public class Alarm
    {
        [Key(0)]
        public int ID { get; set; }

        [Key(1)]
        public DateTime TimeStamp { get; set; }

        [Key(2)]
        public string Text { get; set; }
    }
}
