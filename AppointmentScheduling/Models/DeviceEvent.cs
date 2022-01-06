using System;
using System.ComponentModel.DataAnnotations;

namespace AppointmentScheduling.Models
{
    public class DeviceEvent
    {
        [Key]
        public int id { get; set; }
        public int DeviceId { get; set; }
        public int EventTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsConfirmed { get; set; }

        public Device Device { get; set; }
        public EventType EventType { get; set; }
    }
}