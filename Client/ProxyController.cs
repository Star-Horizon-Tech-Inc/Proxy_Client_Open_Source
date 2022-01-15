using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Client
{
    public class ProxyController
    {
        public RestClient CL;
        public ProxyController()
        {
            CL = new RestClient("http://127.0.0.1:9090");
        }

        public List<string> Get_Proxies()
        {
            JObject jsp = JsonConvert.DeserializeObject<JObject>(CL.Execute(new RestRequest("/proxies", Method.GET)).Content);
            var pr = jsp["proxies"]["GLOBAL"]["all"];
            List<string> proxies = new List<string>();
            foreach (var item in pr)
            {
                if (item.ToString() is "REJECT")
                {
                }
                else
                {
                    proxies.Add(item.ToString());
                }
            }
            return proxies;
        }
        
        public int Change_Proxy(string proxy)
        {
            var resp = CL.Execute(new RestRequest("/proxies/" + proxy, Method.GET));
            if ((int)resp.StatusCode == 200)
            {
                return 0x9;
            }
            else if((int)resp.StatusCode == 400)
            {
                return 0x11;
            }
            else if((int)resp.StatusCode == 404)
            {
                return 0x10;
            }
            else
            {
                return 0x12;
            }
        }

        public static void Start_Proxy()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registryKey.SetValue("ProxyEnable", 1);
            FlushOs();
        }

        public static void Stop_Proxy()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", true);
            registryKey.SetValue("ProxyEnable", 0);
            FlushOs();
        }

        public JObject Get_Speed()
        {
            return JsonConvert.DeserializeObject<JObject>(CL.Execute(new RestRequest("/traffic", Method.GET)).Content);
        }

        static void FlushOs()
        {
            [DllImport("wininet.dll")]
            static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);
            const int INTERNET_OPTION_SETTINGS_CHANGED = 39;
            const int INTERNET_OPTION_REFRESH = 37;
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);

            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
        }
    }
}
