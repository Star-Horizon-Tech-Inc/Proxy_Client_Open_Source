using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Client
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {

        public Home()
        {
            this.InitializeComponent();
            if (App.select_value == null) {LoadProxies(); }
            else { LoadProxies(); Select_Server.SelectedItem = App.select_item; Select_Server.SelectedValue = App.select_value; };
            LoadSub();
            if(U_Control_Status.IsOn != App.ToggleSwitch_Status)
            {
                U_Control_Status.IsOn = App.ToggleSwitch_Status;
            }
        }

        private async void LoadSub()
        {
            if (string.Equals(App.panel_type, "v2board", StringComparison.OrdinalIgnoreCase))
            {
                BasicAuth.Req_Body req = await Task.Run(() => { return App.Auth.User_Request("/api/v1/user/getSubscribe"); });
                if (req.code == 0x13)
                {
                    Sub_Detail = (JObject)req.msg;
                    traffic_used = await Task.Run(() => { return Math.Round(((Convert.ToDouble(Sub_Detail["data"]["d"]) + Convert.ToDouble(Sub_Detail["data"]["d"])) / 1073741824), 2); });
                    traffic_all = await Task.Run(() => { return Math.Round(Convert.ToDouble(Sub_Detail["data"]["transfer_enable"]) / 1073741824, 2); });
                    used_all.Text = traffic_used + "/" + traffic_all + " GB";
                    sub_name.Text = Sub_Detail["data"]["plan"]["name"].ToString();
                    try { sub_exp.Text = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(Convert.ToInt64(Sub_Detail["data"]["expired_at"])).ToString(); }
                    catch { sub_exp.Text = "该订阅长期有效"; }
                    finally { }
                    used_bar.Value = await Task.Run(() => { return Math.Round(traffic_used/traffic_all, 2); });
                }
            }
            else
            {
                BasicAuth.Req_Body req = await Task.Run(() => { return App.Auth.User_Request("/getuserinfo"); });
                if (req.code == 0x13)
                {
                    Sub_Detail = (JObject)req.msg;
                    traffic_used = await Task.Run(() => { return Math.Round((Convert.ToDouble(Sub_Detail["info"]["user"]["lastUsedTraffic"]) / 1073741824), 2); });
                    traffic_all = await Task.Run(() => { return Math.Round(Convert.ToDouble(Sub_Detail["info"]["user"]["auto_reset_bandwidth"]) / 1073741824, 2); });
                    used_all.Text = traffic_used + "/" + traffic_all + " GB";
                    sub_name.Text = Sub_Detail["info"]["user"]["class"].ToString();
                    try
                    {
                        sub_exp.Text = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds(Convert.ToInt64(Sub_Detail["info"]["user"]["class_expire"])).ToString();
                    }
                    catch { sub_exp.Text = "该订阅长期有效"; }
                    finally { }
                    used_bar.Value = await Task.Run(() => { return Math.Round(traffic_used / traffic_all, 2); });


                }
            }
        }
        public async void LoadProxies()
        {
            Node_CH = await Task.Run(()=> { return new ProxyController().Get_Proxies(); });
            Select_Server.ItemsSource = Node_CH;
        }
        public static List<string> Node_CH { get; set; }

        private async void Select_Server_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Select_Server.SelectedItem != App.select_item)
            {
                App.select_item = Select_Server.SelectedItem;
                App.select_value = Select_Server.SelectedValue;
                int req = await Task.Run(() => { return new ProxyController().Change_Proxy(App.select_item.ToString()); });
                if (req == 0x10)
                {
                    Change_Fail.IsOpen = true;
                }
                else if (req == 0x11)
                {
                    Clash_Err.IsOpen = true;
                }
            }
            
        }
        public static JObject Sub_Detail { get; set; }
        public static double traffic_used { get; set; }
        public static double traffic_all { get; set; }
        public void ToggleSwitch_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn != App.ToggleSwitch_Status)
                {
                    if (toggleSwitch.IsOn == true)
                    {
                        App.ToggleSwitch_Status = true;
                        App.Start_Proxy();
                    }
                    else
                    {
                        App.ToggleSwitch_Status = false;
                        App.Stop_Proxy();
                    }
                }
            }
        }

    }

}
