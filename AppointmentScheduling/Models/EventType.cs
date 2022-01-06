using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppointmentScheduling.Models
{
    public class EventType
    {

        public EventType()
        {
            this.DeviceEvent = new HashSet<DeviceEvent>();
        }

        [Key]
        public int id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsAlarm { get; set; }
        public bool ConfirmReq { get; set; }
        public string Command { get; set; }
        public string Procedura { get; set; }

        public virtual ICollection<DeviceEvent> DeviceEvent { get; set; }
    }
}