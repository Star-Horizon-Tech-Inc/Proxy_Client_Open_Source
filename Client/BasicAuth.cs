using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client
{
    public struct Paths
    {
        public string login;
        public string register;
        public string send;
        public string type;
        public string node_info;
        public string getshops;
    }

    public class BasicAuth
    {
        private string cookies { get; set; }
        public Paths path;
        public RestClient res_cl { get; set; }
        private string uri { get; set; }
        public BasicAuth(string url, string panel)
        {
            uri = url;
            res_cl = new RestClient(url);
            res_cl.CookieContainer = new CookieContainer();
            if (string.Equals(panel, "v2board", StringComparison.OrdinalIgnoreCase))
            {
                path.login = "/api/v1/passport/auth/login";
                path.register = "/api/v1/passport/auth/register";
                path.send = "/api/v1/passport/comm/sendEmailVerify";
                path.node_info = "/api/v1/user/server/fetch";
                path.getshops = "/api/v1/user/plan/fetch";
                path.type = "v2board";
            }
            else
            {
                path.login = "/auth/login";
                path.register = "/auth/register";
                path.send = "/auth/send";
                path.node_info = "/getnodelist";
                path.type = "sspanel";
            }
        }

        public int Login(string account, string password, string code = "")
        {
            var req = new RestRequest(path.login, Method.POST);
            req.AddParameter("email", account);
            req.AddParameter("password", password);
            req.AddParameter("passwd", password);
            req.AddParameter("code", code);
            var resp = res_cl.Execute(req);
            if ((int)resp.StatusCode == 200)
            {
                if (string.Equals(path.type, "v2board", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var item in resp.Headers)
                    {
                        if (item.Name == "Set-Cookie")
                        {
                            cookies = item.Value.ToString();
                            res_cl.CookieContainer.SetCookies(new Uri(uri), cookies);
                            return 0x6;
                        }
                    }
                    return 0x1;
                }
                else
                {
                    var json_resp = JsonConvert.DeserializeObject<SSP_Msg>(resp.Content);
                    if (json_resp.ret == 0)
                    {
                        return 0x2;
                    }
                    else
                    {
                        foreach (var item in resp.Headers)
                        {
                            if (item.Name == "Set-Cookie")
                            {
                                cookies = item.Value.ToString();
                                res_cl.CookieContainer.SetCookies(new Uri(uri), cookies);
                                return 0x6;
                            }
                        }
                        return 0x1;
                    }
                }
            }
            else
            {
                return 0x6;
            }
        }

        public int Register(string email, string password, string email_code, string invite_code, string name = "")
        {
            RestRequest req = new RestRequest(path.register, Method.POST);
            req.AddParameter("email", email);
            req.AddParameter("password", password);
            req.AddParameter("auth_password", password);
            req.AddParameter("repassword", password);
            req.AddParameter("emailcode", email_code);
            req.AddParameter("email_code", email_code);
            req.AddParameter("invite_code", invite_code);
            req.AddParameter("name", name);
            var resp = res_cl.Execute(req);
            if ((int)resp.StatusCode == 200)
            {
                if (string.Equals(path.type, "v2board", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var item in resp.Headers)
                    {
                        if (item.Name == "Set-Cookie")
                        {
                            cookies = item.Value.ToString();
                            return 0x7;
                        }
                    }
                    return 0x1;
                }
                else
                {
                    var json_resp = JsonConvert.DeserializeObject<SSP_Msg>(resp.Content);
                    if (json_resp.ret == 1)
                    {
                        foreach (var item in resp.Headers)
                        {
                            if (item.Name == "Set-Cookie")
                            {
                                cookies = item.Value.ToString();
                                return 0x7;
                            }
                        }
                        return 0x1;
                    }
                    else
                    {
                        switch (json_resp.msg)
                        {
                            case "您的邮箱验证码不正确":
                                return 0x3;
                            default:
                                return 0x1;
                        }
                    }
                }
            }
            else
            {
                if (string.Equals(path.type, "v2board", StringComparison.OrdinalIgnoreCase))
                {
                    var json_resp = JsonConvert.DeserializeObject<V2board_Msg>(resp.Content);
                    switch (json_resp.message)
                    {
                        case "邮箱验证码有误":
                            return 0x3;
                        case "The given data was invalid.":
                            return 0x5;
                        case "邀请码无效":
                            return 0x4;
                        default:
                            return 0x1;
                    }
                }
                return 0x1;
            }
        }

        public int Send_Verify_Code(string email)
        {
            RestRequest req = new RestRequest(path.send, Method.POST);
            req.AddParameter("email", email);
            var resp = res_cl.Execute(req);
            if ((int)resp.StatusCode == 500)
            {
                return 0x1;
            }
            else
            {
                return 0x8;
            }
        }

        public Req_Body Get_Nodes_info()
        {
            var node_list = new List<node_info>();
            var resp = res_cl.Execute(new RestRequest(path.node_info, Method.GET));
            if ((int)resp.StatusCode == 200)
            {
                JObject jsp = JsonConvert.DeserializeObject<JObject>(resp.Content);
                if (string.Equals(path.type, "v2board", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        foreach (var item in jsp["data"])
                        {
                            var j_item = JsonConvert.DeserializeObject<JObject>(item.ToString());
                            var info = new node_info();
                            info.node_name = item["name"].ToString();
                            info.node_rate = "x" + item["rate"].ToString() + "倍率  ";
                            node_list.Add(info);
                        }
                        return new Req_Body(0x13, node_list);
                    }
                    catch
                    {
                        return new Req_Body(0x14, "None");
                    }
                }
                else
                {
                    try
                    {
                        foreach (var item in jsp["nodeinfo"]["nodes"])
                        {
                            var j_item = JsonConvert.DeserializeObject<JObject>(item.ToString());
                            var info = new node_info();
                            info.node_name = item["name"].ToString();
                            info.nodeinfo = item["info"].ToString();
                            info.node_rate = "x" + item["traffic_rate"].ToString() + "倍率  ";
                            node_list.Add(info);
                        }
                        return new Req_Body(0x13, node_list);
                    }
                    catch
                    {
                        return new Req_Body(0x14, "None");
                    }
                }
            }
            else { return new Req_Body(0x1, "error"); }
        }

        public Req_Body Get_Shops()
        {
            var shop_list = new List<Shops>();

            var resp = res_cl.Execute(new RestRequest(path.node_info, Method.GET));
            if ((int)resp.StatusCode == 200)
            {
                JObject jsp = JsonConvert.DeserializeObject<JObject>(resp.Content);
                if (string.Equals(path.type, "v2board", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        foreach (var item in jsp["data"])
                        {
                            var j_item = JsonConvert.DeserializeObject<JObject>(item.ToString());
                            var info = new Shops();
                            info.Name = item["name"].ToString();
                            info.Price= Math.Round(Convert.ToDouble(item["month_price"].ToString())/100 ,2).ToString()+"元";
                            info.Content = item["content"].ToString();
                            shop_list.Add(info);
                        }
                        return new Req_Body(0x13, shop_list);
                    }
                    catch
                    {
                        return new Req_Body(0x14, "None");
                    }
                }
                else
                {
                    return new Req_Body(0x1, "error");
                }
            }
            else { return new Req_Body(0x1, "error"); }
        }

        public class V2board_Msg
        {
            public string message { get; set; }

            public Errors errors { get; set; }

            public class Errors
            {
                public List<string> email { get; set; }
            }
        }

        public class SSP_Msg
        {
            public int ret { get; set; }

            public string msg { get; set; }
        }

        public Req_Body User_Request(string url)
        {
            int code;
            var req = new RestRequest(url, Method.GET);
            var resp = res_cl.Execute(req);
            if ((int)resp.StatusCode == 200) { code = 0x13; }
            else { code = 0x1; }
            Req_Body rtn_msg = new Req_Body(code, JsonConvert.DeserializeObject<JObject>(resp.Content));
            return rtn_msg;
        }

        public class Req_Body
        {
            public Req_Body(int co, Object ms)
            {
                code = co;
                msg = ms;
            }
            public int code { get; set; }
            public Object msg { get; set; }
        }
    }
    public class node_info
    {
        public string node_name { get; set; }
        public string nodeinfo = "None";
        public string node_rate { get; set; }
    }

    public class Shops
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string Price { get; set; }
    }
}
