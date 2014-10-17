using System;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace Whir.Framework
{
    /// <summary>
    /// 新浪IP查询
    /// </summary>
    public static class IPHelper
    {
        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);

        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);

        /// <summary>
        /// 取得客户端IP
        /// </summary>
        /// <returns></returns>
        public static string GetClientIP()
        {
            HttpRequest request = HttpContext.Current.Request;
            const string headerKeyIP = "X-Forwarded-For";
            string ip = string.Empty;
            string ipHeader = request.Headers[headerKeyIP];

            if (!string.IsNullOrEmpty(ipHeader))
            {
                string[] ips = ipHeader.Split(',');
                foreach (string ipItem in ips)
                {
                    if ((!string.IsNullOrEmpty(ipItem)) && (!IsLocalIP(ipItem)))
                    {
                        ip = ipItem;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(ip))
            {
                string strHostName = Dns.GetHostName();
                ip = Dns.GetHostAddresses(strHostName).GetValue(0).ToString();
            }
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.UserHostAddress;
            }
            return ip;
        }

        private static bool IsLocalIP(string ip)
        {
            return ip.StartsWith("192.168.") || ip.StartsWith("172.16.") || ip.StartsWith("10.");
        }

        /// <summary>
        /// 取得客户端外网IP
        /// </summary>
        /// <returns></returns>
        public static string GetClientWebIP()
        {
            try
            {
                const string ip138ComIcAsp = "http://scripts.hashemian.com/js/visitorIPHOST.js.php"; //查询外网IP
                var uri = new Uri(ip138ComIcAsp);
                WebRequest wr = WebRequest.Create(uri);
                Stream stream = wr.GetResponse().GetResponseStream();
                if (stream != null)
                {
                    var reader = new StreamReader(stream, Encoding.Default);
                    string result = reader.ReadToEnd(); 
                    var regex = new Regex("(?<First>2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.(?<Second>2[0-4]\\d" +
                                          "|25[0-5]|[01]?\\d\\d?)\\.(?<Third>2[0-4]\\d|25[0-5]|[01]?\\d" +
                                          "\\d?)\\.(?<Fourth>2[0-4]\\d|25[0-5]|[01]?\\d\\d?)",
                                          RegexOptions.IgnoreCase | RegexOptions.CultureInvariant |
                                          RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
                    Match ip = regex.Matches(result)[0];
                    return ip.ToString();
                }
            }
            catch (Exception ex)
            {
            }
            return string.Empty;
        }

        public static IpDetail GetIpDetail()
        {
            string ip = GetClientIP();
            if (ip.IsIP())
            {
                return GetIpDetail(ip);
            }
            return new IpDetail();
        }

        /// <summary>
        /// 获取IP地址的详细信息，调用的接口为
        /// http://int.dpool.sina.com.cn/iplookup/iplookup.php?format=json&ip={ip}
        /// </summary>
        /// <param name="ipAddress">请求分析得IP地址</param>
        /// <returns>IpUtils.IpDetail</returns>
        public static IpDetail GetIpDetail(string ipAddress)
        {
            string ip = ipAddress;
            Encoding sourceEncoding = Encoding.UTF8;
            using (
                Stream receiveStream =
                    WebRequest.Create("http://int.dpool.sina.com.cn/iplookup/iplookup.php?format=json&ip=" + ipAddress).
                        GetResponse().GetResponseStream())
            {
                if (receiveStream != null)
                    using (var sr = new StreamReader(receiveStream, sourceEncoding))
                    {
                        var readbuffer = new char[256];
                        int n = sr.Read(readbuffer, 0, readbuffer.Length);
                        int realLen = 0;
                        while (n > 0)
                        {
                            realLen = n;
                            n = sr.Read(readbuffer, 0, readbuffer.Length);
                        }
                        ip = ConvertToGb(sourceEncoding.GetString(sourceEncoding.GetBytes(readbuffer, 0, realLen)));
                    }
            }
            var ipDetail = new IpDetail();
            try
            {
                ipDetail = JavaScriptConvert.DeserializeObject<IpDetail>(ip);
                ipDetail.ip = ipAddress;
            }
            catch
            {
                ipDetail.city = "未知";
            }
            return ipDetail;
        }

        /// <summary>
        /// 把Unicode解码为普通文字
        /// </summary>
        /// <param name="unicodeString">要解码的Unicode字符集</param>
        /// <returns>解码后的字符串</returns>
        public static string ConvertToGb(string unicodeString)
        {
            var regex = new Regex(@"\\u\w{4}");
            MatchCollection matchs = regex.Matches(unicodeString);
            foreach (Match match in matchs)
            {
                string tempvalue = char.ConvertFromUtf32(Convert.ToInt32(match.Value.Replace(@"\u", ""), 16));
                unicodeString = unicodeString.Replace(match.Value, tempvalue);
            }
            return unicodeString;
        }

        /// <summary>
        /// 取得本机Mac地址 
        /// </summary>
        /// <returns></returns>
        public static string GetServerMac()
        {
            string mac = "";
            var mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection mos = mc.GetInstances();
            foreach (ManagementObject mo in mos)
            {
                if ((bool) mo["IPEnabled"])
                {
                    mac += mo["MACAddress"].ToString();
                    return mac;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 取得客户端Mac地址
        /// </summary>
        /// <returns></returns>
        public static string GetClientMac()
        {
            try
            {
                //待实现
                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }

    public class IpDetail
    {
        public string ret { get; set; }

        public string start { get; set; }

        public string end { get; set; }

        public string country { get; set; }

        public string province { get; set; }

        public string city { get; set; }

        public string district { get; set; }

        public string isp { get; set; }

        public string type { get; set; }

        public string desc { get; set; }

        public string ip { get; set; }
    }
}