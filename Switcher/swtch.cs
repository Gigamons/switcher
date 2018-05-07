using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Switcher.Properties;

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
                X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, "Gigamons", true);

                string certFile = Path.Combine(Path.GetTempPath() + "cert.crt");

                if (certs.Count == 0)
                {
                    File.WriteAllBytes(certFile, Resources.cert);
                    X509Certificate2Collection collection = new X509Certificate2Collection();
                    collection.Import(certFile);
                    foreach (X509Certificate2 cert in collection)
                        store.Add(cert);

                    File.Delete(certFile);
                }

                List<string> hostc = new List<string>();
                hostc.Add(""); // Fix whitespace with whitespace.
                foreach (IP ip in ips)
                {
                    hostc.Add(ip.ip.Trim() + " " + ip.hostname.Trim());
                }
                File.AppendAllLines(hostsFile, hostc);
            }
            else
            {
                string[] hosts = File.ReadAllLines(hostsFile);
                List<string> hostc = new List<string>();

                foreach (string hostf in hosts)
                {
                    if (hostf.Contains("ppy.sh") && !hostf.StartsWith("#"))
                        continue;
                    hostc.Add(hostf);
                }
                File.WriteAllLines(hostsFile, hostc);
            }
        }

        private byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
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
