using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.IO;

namespace Switcher
{
    class swtch
    {
        private bool Debug;
        private static string hostsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
        public swtch(bool Debug = false)
        {
            this.Debug = Debug;
        }

        public async void Switch()
        {
            IP[] ips;
            string host = "";
            try
            {
                ips = await GetIP_M();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + "\r\n " + ex.StackTrace);
                ips = new IP[6];
                ips[0] = new IP() { hostname = "a.ppy.sh", ip = "127.0.0.1"};
                ips[1] = new IP() { hostname = "s.ppy.sh", ip = "127.0.0.1" };
                ips[2] = new IP() { hostname = "i.ppy.sh", ip = "127.0.0.1" };
                ips[3] = new IP() { hostname = "c.ppy.sh", ip = "127.0.0.1" };
                ips[4] = new IP() { hostname = "c1.ppy.sh", ip = "127.0.0.1" };
                ips[5] = new IP() { hostname = "osu.ppy.sh", ip = "127.0.0.1" };
            }

            if (!isSwitched())
            {
                foreach (IP ip in ips)
                {
                    host += ip.ip + " " + ip.hostname + "\r\n";
                }
                File.AppendAllText(hostsFile, host);
            }
            else
            {
                string[] hosts = File.ReadAllLines(hostsFile);
                foreach (string hostf in hosts)
                {
                    if (host.Contains("ppy.sh") && !host.StartsWith("#"))
                        continue;

                    host += hostf;
                }
                File.WriteAllText(hostsFile, host);
            }
            
        }

        public static bool isSwitched()
        {
            string[] hosts = File.ReadAllLines(hostsFile);
            foreach(string host in hosts)
            {
                if(host.Contains("ppy.sh") && !host.StartsWith("#"))
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<IP[]> GetIP_M()
        {
            IP[] iplist;
            WebClient client = new WebClient();
            string data = await client.DownloadStringTaskAsync("http://ip.gigamons.de");

            string[] ips = data.Split(',');

            iplist = new IP[ips.Length];

            for (var i = 0; i < ips.Length; i++)
            {
                string[] d = ips[i].Split('|');
                iplist[i] = new IP { hostname = d[0], ip = d[1] };
            }

            return iplist;
        }
    }

    class IP
    {
        public string ip;
        public string hostname;
    }
}
