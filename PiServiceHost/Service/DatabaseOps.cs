using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace PiServiceHost.Service
{

    //Ad data to database
    public class DatabaseOps
    {

        public static void AddPerformDatabaseOperations()
        {
            using (var db = new DeviceDbContext())
            {
                var events = new DeviceEvent
                {
                    DeviceId = 1,
                    Name = "Event",
                    Description = "EventDesc",
                    Time = DateTime.Now,
                    IsDeleted = false,
                    EventTypeId = 1
                };

                db.DeviceEvents.Add(events);
                db.SaveChanges();
            }
        }





    }
}
