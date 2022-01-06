using System.ComponentModel.DataAnnotations;

namespace AppointmentScheduling.Models
{
    public class Device
    {

        [Key]
        public int id { get; set; }
        public string UID { get; set; }
        public string? Name { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Connectivity { get; set; }
        public bool IsOnline { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
        public string RoundtripTime { get; set; }
        public string Ttl { get; set; }
        public string DontFragment { get; set; }
        public string Length { get; set; }

    }
}