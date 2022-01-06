using Serilog;
using PiService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Data.Entity.Migrations;


namespace ServiceHost.ServiceCore
{
    public class NetworkOps
    {
        string range;

        public void ViewAllDbDevices()
        {
            using (var db = new DeviceDbContext())
            {
                var stdQuery = (from d in db.Device.Where(d => d.IsEnabled == true)
                                select new { Manufacturer = d.Manufacturer, Address = d.Address, Name = d.Name });
                foreach (var s in stdQuery)
                {
                    Console.WriteLine("{0} {1} {2}", s.Manufacturer, s.Address, s.Name);
                }

            }
        }

        //Get all database devices
        public List<Device> GetAllDbDevices()
        {
            List<Device> list = new List<Device>();
            using (var db = new DeviceDbContext())
            {
                var stdQuery = (from d in db.Device.Where(d => d.IsEnabled == true)
                                select d).ToList();
                foreach (var item in stdQuery)
                {
                    list.Add(item);
                }
                return list;
            }
        }

        public string GetMachineNameFromIPAddress(string ipAdress)
        {
            string machineName = string.Empty;
            try
            {
                var hostEntry = Dns.GetHostEntry(ipAdress);

                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Machine not found...");
            }
            return machineName;
        }

        //check mac metoda
        public List<string> GetMacList()
        {
            List<string> list = new List<string>();
            using (var db = new DeviceDbContext())
            {
                var macQuery = (from d in db.Device.Where(d => d.IsEnabled == true).Where(d => d.IsOnline)
                                select new { Manufacturer = d.Manufacturer });

                foreach (var item in macQuery)
                {
                    //Console.WriteLine("{0} metoda" ,item.Manufacturer);
                    list.Add(item.Manufacturer);
                }
                return list;
            }
        }

        public List<string> GetIPList()
        {
            List<string> list = new List<string>();
            using (var db = new DeviceDbContext())
            {
                var ipQuery = (from d in db.Device.Where(d => d.IsEnabled == true).Where(d => d.IsOnline == true)
                               select new { Address = d.Address });
                foreach (var item in ipQuery)
                {
                    list.Add(item.Address);
                }
                return list;
            }
        }

        public List<string> GetHostNameList()
        {
            List<string> list = new List<string>();
            using (var db = new DeviceDbContext())
            {
                var hnQuery = (from d in db.Device.Where(d => d.IsEnabled == true).Where(d => d.IsOnline == true)
                               select new { Name = d.Name });
                foreach (var item in hnQuery
                )
                {
                    list.Add(item.Name);
                }
                return list;
            }
        }

        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                             + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                             + "-" + substrings[7] + "-"
                             + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "not found";
            }
        }

        public string GetRange(string myIP)
        {

            string temp = myIP;
            string reverse = "";

            for (int i = temp.Length - 1; i >= 0; i--)
            {
                reverse += temp[i];
            }

            int ipLenght = reverse.Length;
            int indexDot = reverse.IndexOf(".");
            string revClean = reverse.Substring(indexDot, ipLenght - indexDot);
            temp = "";
            for (int i = revClean.Length - 1; i >= 0; i--)
            {
                temp += revClean[i];
            }
            range = temp;
            //Console.WriteLine("Range is: {0}", range);
            return range;
        }
        //Get my ip
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    //return "192.168.1.13";
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        public bool IsOnline()
        {

            return true;
        }

       public Device PingDevice(string ipAddress)
        {
            var deviceList = GetAllDbDevices();
            string targetHost = ipAddress;
            string data = "a quick brown fox jumped over the lazy dog";
            Ping pingSender = new Ping();
            Device device = new Device();
            PingOptions options = new PingOptions
            {
                DontFragment = true
            };

            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 5000;

            //Console.WriteLine($"Pinging {targetHost}");
            PingReply reply = pingSender.Send(targetHost, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                device.Address = reply.Address.ToString();
                device.Name = GetMachineNameFromIPAddress(targetHost);
                device.Manufacturer = GetMacAddress(targetHost);
                device.RoundtripTime = reply.RoundtripTime.ToString();
                device.Ttl = reply.Options.Ttl.ToString();
                device.DontFragment = reply.Options.DontFragment.ToString();
                device.Length = reply.Buffer.Length.ToString();
                device.IsOnline = true;
                //Console.WriteLine($"Address: {device.Address},\tHostName: {device.Name},\tRoundTrip time: {device.RoundtripTime},\tTTL: {device.Ttl}");
                return device;
            }
            else if (reply.Status != IPStatus.Success)
            {
                //Console.WriteLine(reply.Status);
                device.IsOnline = false;
                return device;
            }
            else
            {

                device.IsOnline = false;
                return device;
            }
        }

        //Scan all devices, traženje novih uređaja kod pokretanja
        public void ScanDevices()
        {
            using (var db = new DeviceDbContext())
            {
                string hostName = Dns.GetHostName(); // Retrive the Name of HOST
                Console.WriteLine("My host name is: {0}", hostName);
                // Get the IP

                string myIP = GetLocalIPAddress();
                Console.WriteLine("My IP is: {0}", myIP);
                var deviceList = GetAllDbDevices();
                var ipList = GetIPList();
                var macList = GetMacList();
                var hostList = GetHostNameList();

                for (int i = 1; i < 254; i++)
                {
                        string pingIP = GetRange(myIP) + i.ToString();
                        var devicePing = PingDevice(pingIP);
                        string ip = Convert.ToString(devicePing.Address);
                        string host = Convert.ToString(devicePing.Name);
                        string mac = Convert.ToString(devicePing.Manufacturer);


                    if (devicePing.Manufacturer == null)
                    {
                        continue;
                    }
                    foreach (var item in deviceList)
                    {
                        if (item.Manufacturer.Contains(mac))
                        {
                            Console.WriteLine("Uređaj {0} @ {1} u bazi: ", mac, ip);
                            continue;
                        }
                    };
                    

                        if (macList.Contains(mac) && !(hostList.Contains(host)))
                        {
                            Console.WriteLine("Uređaj {0} @ {1} u bazi ima drugi host name: ", mac, host);
                            continue;
                        }
                        else if(!macList.Contains(mac))
                        {

                            Console.WriteLine("Novi uređaj!");

                        
                            Device newDevice = new Device();
                            {
                                newDevice.UID = "TestUID";
                                newDevice.Name = host;
                                newDevice.Manufacturer = mac;
                                newDevice.Model = "TestModel";
                                newDevice.Address = range + i;
                                newDevice.Username = "Scaned";
                                newDevice.Password = "TestPass";
                                newDevice.Connectivity = 1;
                                newDevice.IsOnline = true;
                                newDevice.IsEnabled = true;
                                newDevice.IsDeleted = false;
                                newDevice.RoundtripTime = devicePing.RoundtripTime;
                                newDevice.Ttl = devicePing.Ttl;
                                newDevice.DontFragment = devicePing.DontFragment;
                                newDevice.Length = devicePing.Length;
                            };
                            db.Device.Add(newDevice);

                        
                        //foreach pristup sa device full, lakše updejtat


                        //Console.WriteLine("{0} @ {1} {2} @ {3} {4} {5} {6} {7}", DateTime.Now, device.Address, device.Name, device.Manufacturer, device.Ttl, device.DontFragment, device.Length, device.RoundtripTime.ToString());

                        db.SaveChanges();

                        }
                }

                Console.WriteLine("Provjera duplih MAC: ");
                if (CheckForDuplicateMac() == true)
                {
                    Console.WriteLine("Postoje online korisnici sa istom MAC adresom!!!");
                    //distinct i id tablica (mac, broj kartice, qr... na dvije razine osigurati, ukoliko se dogodi, prebaci na true prop u id tablici na retku tipa )
                }

            }
        }

        //Check for double mac
        public bool CheckForDuplicateMac()
        {
            var macList = GetMacList();
            if (macList.Count != macList.Distinct().Count())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Check for ip conflict
        public bool CheckForDuplicateIp()
        {
            var ipList = GetIPList();
            if (ipList.Count != ipList.Distinct().Count())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Check all database devices
        public void PingAllDevices()
        {
            using (var db = new DeviceDbContext())
            {

                var deviceList = GetAllDbDevices();
                foreach (var q in deviceList)
                {

                    var devicePing = PingDevice(q.Address);
                    

                    if ((q.IsOnline == false) && (devicePing.IsOnline == false))
                        {
                            if (q.IsOnline == true)
                            {
                            var online = deviceList.FirstOrDefault(d => d.id == q.id);
                            Console.WriteLine(online.IsOnline);
                            Console.WriteLine("Device is offline");
                                online.IsOnline = false;
                                db.Device.AddOrUpdate(online);
                                db.SaveChanges();
                            continue;
                            }
                        }
                    
                    if ((q.IsOnline == true) && (devicePing.IsOnline == false))
                        {
                        var online = deviceList.FirstOrDefault(d => d.id == q.id);
                        Console.WriteLine(online.IsOnline);
                            Console.WriteLine("Device {0} went offline", q.Name);
                            Log.Information("Device {0} went offline", q.Name);
                            //create event online
                            DeviceEvent deviceEvent = new DeviceEvent();
                            {
                                deviceEvent.EventTypeId = 2;
                                deviceEvent.Name = "Offline";
                                deviceEvent.Description = $"Device {q.Name} went offline";
                                deviceEvent.Time = DateTime.Now;
                                deviceEvent.IsDeleted = false;
                                deviceEvent.IsConfirmed = false;
                                deviceEvent.DeviceId = q.id;
                                //treba updejt device IsOnline
                            };

                            online.IsOnline = false;
                            db.Device.AddOrUpdate(online);
                            db.DeviceEvents.Add(deviceEvent);
                            db.SaveChanges();
                            continue;
                        }

                    


                    

                    if ((q.IsOnline == false) && (devicePing.IsOnline == true))
                    {
                        var online = deviceList.FirstOrDefault(d => d.id == q.id);

                            Console.WriteLine("Device {0} went online", q.Name);
                            Log.Information("Device {0} went online", q.Name);

                            DeviceEvent deviceEvent = new DeviceEvent();
                            {
                                deviceEvent.EventTypeId = 1;
                                deviceEvent.Name = "Online";
                                deviceEvent.Description = $"Device {q.Name} went online";
                                deviceEvent.Time = DateTime.Now;
                                deviceEvent.IsDeleted = false;
                                deviceEvent.IsConfirmed = false;
                                deviceEvent.DeviceId = q.id;

                            };
                        online.IsOnline = true;
                        db.Device.AddOrUpdate(online);
                        db.DeviceEvents.Add(deviceEvent);
                        db.SaveChanges();
   
                    }    
                }
            }
        }
    }
}
