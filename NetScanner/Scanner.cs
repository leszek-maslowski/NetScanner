using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// using WUApiLib;

namespace NetScanner
{
    public class IpScanner
    {
        Threader threader;

        static byte[] buffer = new byte[32];
        static int timeout = 1000;

        public IpScanner()
        {
            threader = new Threader(64);
        }

        public IEnumerable<ScannerHost> Scan()
        {
            return Scan(GetInterfaceIpRanges().ToArray());
        }

        public IEnumerable<ScannerHost> Scan(params IPRange[] ranges)
        {
            List<ScannerHost> list = new List<ScannerHost>();
            foreach (IPRange r in ranges)
            {
                foreach (IPAddress ip in GetAddresses(ByteArrayToInt(r.From.GetAddressBytes()), ByteArrayToInt(r.To.GetAddressBytes())))
                {
                    threader.Enqueue(new Action(() =>
                    {
                        ScannerHost h = Scan(ip);
                        if (h != null)
                            lock (list)
                                list.Add(h);
                    }));
                }
            }

            while (threader.Busy())
            {
                lock (list)
                    if (list.Count > 0)
                    {
                        foreach (ScannerHost h in list)
                            yield return h;
                    }
                Thread.Sleep(20);
            }
        }

        public ScannerHost Scan(IPAddress ip)
        {
            int ms = Ping(ip);
            if (ms >= 0)
            {
                IPHostEntry entry = null;
                Try(() => entry = Dns.GetHostEntry(ip));
                ScannerHost h = new ScannerHost()
                {
                    Address = ip.ToString(),
                    Name = (entry == null || !entry.AddressList.Contains(ip)) ? null : entry.HostName
                };

                if (!string.IsNullOrEmpty(h.Name))
                {
                    Try(() =>
                    {
                        ManagementScope scope = new ManagementScope(string.Format("\\\\{0}\\root\\cimv2", h.Name));
                        Try(() => scope.Connect());

                        ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                        ManagementObjectCollection queryCollection = searcher.Get();
                        ManagementObject m = queryCollection.Cast<ManagementObject>().First();

                        h.OS = string.Format("{0}", m["Caption"]);
                        h.ComputerName = string.Format("{0}", m["CSName"]);

                    });
                }
                return h;
            }
            return null;
        }

        public static bool CheckUpdates(string machineName)
        {
            int updateCount = 0;
            Try(() =>
            {
                //Type typeFromProgID = Type.GetTypeFromProgID("Microsoft.Update.Session", machineName);
                //UpdateSession updateSession = (UpdateSession)Activator.CreateInstance(typeFromProgID);
                //IUpdateSearcher updateSearcher = updateSession.CreateUpdateSearcher();
                //ISearchResult searchResult = updateSearcher.Search("IsInstalled=0  And IsHidden=0 and Type='Software'");

                //IUpdate3[] updates = searchResult.Updates.Cast<IUpdate3>().Where(x => !x.BrowseOnly).ToArray();
                //updateCount = updates.Length;
            });
            return updateCount > 0;
        }

        static int Ping(IPAddress ip)
        {
            using (Ping pingSender = new Ping())
            {
                int result = -1;

                Try(() =>
                {
                    PingReply reply = pingSender.Send(ip, timeout, buffer,
                        new PingOptions(64, true));

                    if (reply.Status == IPStatus.Success)
                        result = (int)reply.RoundtripTime;
                });

                return result;
            }
        }

        static IEnumerable<IPRange> GetInterfaceIpRanges()
        {
            NetworkInterface[] ifaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .Where(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToArray();

            foreach (NetworkInterface adapter in ifaces)
            {
                foreach (UnicastIPAddressInformation i in adapter.GetIPProperties()
                    .UnicastAddresses
                    .Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    yield return IPRange.CreateFromIpAndMask(i.Address, i.IPv4Mask);
                }
            }
        }

        static IEnumerable<IPAddress> GetAddresses(int from, int to)
        {
            unchecked
            {
                for (uint i = (uint)from; i <= (uint)to; i++)
                    yield return new IPAddress(IntToByteArray((int)i));
            }
        }

        static int ByteArrayToInt(byte[] b)
        {
            return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | (b[3]);
        }

        static byte[] IntToByteArray(int a)
        {
            return new byte[] { (byte)(a >> 24), (byte)(a >> 16 & 0xff), (byte)(a >> 8 & 0xff), (byte)(a & 0xff) };
        }

        static Exception Try(Action a)
        {
            try
            {
                a();
                return null;
            }
            catch (Exception ex)
            {

                return ex;
            }
        }
    }
}
