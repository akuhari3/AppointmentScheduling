using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;


namespace PiServiceHost
{
    public class DeviceDbContext : DbContext
    {
        public DeviceDbContext() : base("name=DefaultConnection")
        {

        }

        public DbSet<Device> Device { get; set; }
        public DbSet<DeviceEvent> DeviceEvents { get; set; }

    }
}
