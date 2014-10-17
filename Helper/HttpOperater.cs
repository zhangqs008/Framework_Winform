using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Whir.Framework.Helper
{
    public class GZipWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            if (request != null)
            {
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate; 
                return request;
            }
            return null;
        }
    }

    /// <summary>
    ///     模拟HTTP操作
    /// </summary>
    public class HttpOperater
    {
        public static string Get(string url)
        {
            using (var client = new GZipWebClient())
            {
                var bytes = client.DownloadData(url);
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }

        /// <summary>
        ///     发起Http请求
        /// </summary>
        /// <param name="httpRequestType">请求方式</param>
        /// <param name="url">请求地址</param>
        /// <param name="cookieInput">请求时传入的cookie</param>
        /// <param name="cookieOutput">服务器返回的cookie</param>
        /// <param name="postData">发送数据</param>
        /// <returns></returns>
        public static string DoRequest(HttpRequestType httpRequestType, string url, string cookieInput,
                                       ref string cookieOutput, string postData)
        {
            string response;
            try
            {
                const string windowsUserName = "";
                const string windowsPwd = "";
                const string userAgent =
                    "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36";
                const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                const string acceptLanguage = "zh-CN,zh;q=0.8";
                const string acceptEncoding = "gzip,deflate,sdch";
                CookieContainer cookieContainer = GetCookie(url, cookieInput);

                var newUri = new Uri(url);
                var request = (HttpWebRequest)WebRequest.Create(newUri);
                request.PreAuthenticate = true;
                if (windowsUserName.Length > 0 & windowsPwd.Length > 0)
                {
                    request.Credentials = new NetworkCredential(windowsUserName.Trim(), windowsPwd.Trim());
                }
                request.Timeout = 20000;
                request.CookieContainer = cookieContainer;
                request.UserAgent = userAgent;
                request.Accept = accept;
                request.Headers["Accept-Language"] = acceptLanguage;
                request.Headers["Accept-Charset"] = acceptEncoding;
                request.Headers["Accept-Encoding"] = acceptEncoding;
                request.Referer = newUri.AbsoluteUri;
                request.Method = httpRequestType == HttpRequestType.GET ? "GET" : "POST";
                if (request.Method == "POST")
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                    byte[] bytes = Encoding.UTF8.GetBytes(postData);
                    request.ContentLength = bytes.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                    }
                }
                using (var webResponse = (HttpWebResponse)request.GetResponse())
                {
                    response =
                        new StreamReader(new GZipStream(webResponse.GetResponseStream(), CompressionMode.Decompress),
                                         Encoding.UTF8).ReadToEnd();
                }
                CookieCollection cookies = cookieContainer.GetCookies(newUri);
                cookieOutput = CookieTostr(cookies);
            }
            catch (NotSupportedException exception)
            {
                response = exception.Message;
            }
            catch (InvalidOperationException exception)
            {
                response = exception.Message;
            }
            catch (IOException exception)
            {
                response = exception.Message;
            }
            catch (Exception exception)
            {
                response = exception.Message;
            }
            return response;
        }


        /// <summary>
        /// 设置cookie域
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="cookieInput">cookie</param>
        /// <returns></returns>
        public static CookieContainer GetCookie(string url, string cookieInput)
        {
            var cookieContainer = new CookieContainer();
            var cookies = new CookieCollection();
            string[] cookiesArr = cookieInput.Split(';');
            foreach (string s in cookiesArr)
            {
                string[] keyValuePair = s.Split('=');
                if (keyValuePair.Length > 1)
                {
                    var cookie = new Cookie
                                     {
                                         Name = keyValuePair[0].Trim(),
                                         Value = keyValuePair[1].Trim(),
                                         Domain = GetDomain(url).Trim() //设置cookie域
                                     };
                    cookies.Add(cookie);
                }
            }
            cookieContainer.Add(cookies);
            return cookieContainer;
        }

        /// <summary>
        /// 通过Url取得域
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetDomain(string url)
        {
            var regex = new Regex("(?i)http[s]*://(?<domain>[\\w|.]*)",
                                  RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return regex.Match(url).Groups["domain"].Value;
        }

        /// <summary>
        /// 将cookie转为字符串
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static string CookieTostr(CookieCollection cookies)
        {
            return cookies.Cast<Cookie>()
                .Aggregate(string.Empty, (current, c) => current + (c.Name + "=" + c.Value + ";"));
        }
    }

    /// <summary>
    ///     HTTP请求方式
    /// </summary>
    public enum HttpRequestType
    {
        /// <summary>
        ///     GET
        /// </summary>
        GET = 1,

        /// <summary>
        ///     POST
        /// </summary>
        POST = 2
    }
}