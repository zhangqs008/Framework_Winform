using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;

namespace Whir.Framework.Helper
{
    /// <summary>
    ///     语言类型
    /// </summary>
    public class LanguageType
    {
        /// <summary>
        ///     中文
        /// </summary>
        public static string Chinese = "zh-cn";

        /// <summary>
        ///     英文
        /// </summary>
        public static string English = "en";
    }

    /// <summary>
    ///     翻译方式类型
    /// </summary>
    public class TranslationType
    {
        /// <summary>
        ///     Google
        /// </summary>
        public static string Google = "GoogleTanslater";

        /// <summary>
        ///     Bing
        /// </summary>
        public static string Bing = "MircsoftTanslater";
    }

    /// <summary>
    ///     语言翻译类
    /// </summary>
    public class TranslaterHelper
    {
        /// <summary>
        ///     翻译方法 中文："zh-cn", 英文："en" type：MircsoftTanslater，GoogleTanslater
        /// </summary>
        /// <param name="sourceText">翻译原文</param>
        /// <param name="fromLanguage">原始语言</param>
        /// <param name="toLanguage">目标语言</param>
        /// <param name="type">翻译API</param>
        /// <returns>译文</returns>
        public static string Translate(string sourceText, string fromLanguage, string toLanguage,
                                       string type = "MircsoftTanslater")
        {
            string translateStr = string.Empty;
            switch (type)
            {
                case "MircsoftTanslater":
                    translateStr = MircsoftTanslater(sourceText, fromLanguage, toLanguage); //"zh-cn", "en";
                    break;
                case "GoogleTanslater":
                    translateStr = GoogleTranslaterPostMethod(sourceText, fromLanguage, toLanguage); //"zh-cn", "en";
                    break;
            }
            return translateStr;
        }

        #region Google 翻译： Get方式获取翻译

        /// <summary>
        ///     Google 翻译： Get方式获取翻译
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        private static string GoogleTranslaterGetMethod(string sourceText, string fromType, string toType)
        {
            string result = null;
            string langPair = (fromType.ToLower() == "zh-cn" || toType.ToLower() == "en-sh") ? "zh|en" : "en|zh";
            string url = "http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&langpair=" +
                         HttpUtility.UrlEncode(langPair) + "&q=" + HttpUtility.UrlEncode(sourceText);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Referer = "http://www.my-ajax-site.com";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding("UTF-8"));
                    string responseStr = reader.ReadToEnd();
                    var readConfig =
                        (ResponseResult)JavaScriptConvert.DeserializeObject(responseStr, typeof(ResponseResult));
                    result = readConfig.ResponseStatus == "200"
                                 ? readConfig.ResponseData.TranslatedText
                                 : readConfig.ResponseStatus;
                
                }
            }
            catch (Exception ex)
            {
                result = "err:" + ex.Message;
            }
            return result;
        }

        #endregion

        #region Google 翻译： Post方式获取翻译

        /// <summary>
        ///     Google 翻译： Post方式获取翻译
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        private static string GoogleTranslaterPostMethod(string sourceText, string fromType, string toType)
        {
            string fromLan = fromType.ToLower() == "zh-cn" ? "zh" : "en";
            string toLan = toType.ToLower() == "zh-cn" ? "zh" : "en";
            var requestScore = (HttpWebRequest)WebRequest.Create("http://translate.google.com/translate_t#");
            var postContent = new StringBuilder();
            Encoding myEncoding = Encoding.UTF8;
            postContent.Append(HttpUtility.UrlEncode("hl", myEncoding));
            postContent.Append("=");
            postContent.Append(HttpUtility.UrlEncode("en", myEncoding));
            postContent.Append("&");
            postContent.Append(HttpUtility.UrlEncode("ie", myEncoding));
            postContent.Append("=");
            postContent.Append(HttpUtility.UrlEncode("UTF-8", myEncoding));
            postContent.Append("&");
            postContent.Append(HttpUtility.UrlEncode("sl", myEncoding));
            postContent.Append("=");
            postContent.Append(HttpUtility.UrlEncode(fromLan, myEncoding));
            postContent.Append("&");
            postContent.Append(HttpUtility.UrlEncode("text", myEncoding));
            postContent.Append("=");
            postContent.Append(HttpUtility.UrlEncode(sourceText, myEncoding));
            postContent.Append("&");
            postContent.Append(HttpUtility.UrlEncode("tl", myEncoding));
            postContent.Append("=");
            postContent.Append(HttpUtility.UrlEncode(toLan, myEncoding));

            byte[] data = Encoding.ASCII.GetBytes(postContent.ToString());
            requestScore.UserAgent =
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            requestScore.Method = "Post";
            //requestScore.ContentType = "application/x-www-form-urlencoded;charset=gb2312";
            requestScore.ContentLength = data.Length;
            requestScore.KeepAlive = true;
            requestScore.Timeout = (6 * 60 * 1000);
            requestScore.ProtocolVersion = HttpVersion.Version10;

            Stream stream = requestScore.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            string content = string.Empty;
            try
            {
                ServicePointManager.Expect100Continue = false;
                var responseSorce = (HttpWebResponse)requestScore.GetResponse();
                stream = responseSorce.GetResponseStream();
                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    content = reader.ReadToEnd();
                    responseSorce.Close();
                    reader.Dispose();
                    stream.Dispose();
                }
            }
            catch (WebException ex)
            {
                var responseSorce = (HttpWebResponse)ex.Response; //得到请求网站的详细错误提示
                if (responseSorce != null)
                {
                    stream = responseSorce.GetResponseStream();
                    if (stream != null)
                    {
                        var reader = new StreamReader(stream);
                        content = reader.ReadToEnd();
                        responseSorce.Close();
                        reader.Dispose();
                        stream.Dispose();
                    }
                }
            }
            finally
            {
                requestScore.Abort();
            }
            const string reg =
                @"<(?<HtmlTag>[\w]+)[^>]*\s[iI][dD]=(?<Quote>[""']?)result_box(?(Quote)\k<Quote>)[""']?[^>]*>((?<Nested><\k<HtmlTag>[^>]*>)|</\k<HtmlTag>>(?<-Nested>)|.*?)*</\k<HtmlTag>>";
            //string reg = @"<(span) id=result_box [^>]*>.*?</\1>";//匹配出翻译内容
            var r = new Regex(reg);
            {
                MatchCollection mcItem = r.Matches(content);
                string result = ConvertHtmlToText(mcItem[0].Value);
                return result;
            }
        }

        /// <summary>
        ///     将HTML转换为纯文本
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ConvertHtmlToText(string source)
        {
            // 代码的实现的思路是：
            //a、先将html文本中的所有空格、换行符去掉（因为html中的空格和换行是被忽略的）
            //b、将<head>标记中的所有内容去掉
            //c、将<script>标记中的所有内容去掉
            //d、将<style>标记中的所有内容去掉
            //e、将td换成空格，tr,li,br,p 等标记换成换行符
            //f、去掉所有以“<>”符号为头尾的标记去掉。
            //g、转换&amp;，&nbps;等转义字符换成相应的符号
            //h、去掉多余的空格和空行
            //remove line breaks,tabs
            string result = source.Replace("\r", " ");
            result = result.Replace("\n", " ");
            result = result.Replace("\t", " ");
            //remove the header
            result = Regex.Replace(result, "(<head>).*(</head>)", string.Empty, RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"<( )*script([^>])*>", "<script>", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"(<script>).*(</script>)", string.Empty, RegexOptions.IgnoreCase);
            //remove all styles
            result = Regex.Replace(result, @"<( )*style([^>])*>", "<style>", RegexOptions.IgnoreCase);
            //clearing attributes
            result = Regex.Replace(result, "(<style>).*(</style>)", string.Empty, RegexOptions.IgnoreCase);
            //insert tabs in spaces of <td> tags
            result = Regex.Replace(result, @"<( )*td([^>])*>", " ", RegexOptions.IgnoreCase);
            //insert line breaks in places of <br> and <li> tags
            result = Regex.Replace(result, @"<( )*br( )*>", "\r", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"<( )*li( )*>", "\r", RegexOptions.IgnoreCase);
            //insert line paragraphs in places of <tr> and <p> tags
            result = Regex.Replace(result, @"<( )*tr([^>])*>", "\r\r", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"<( )*p([^>])*>", "\r\r", RegexOptions.IgnoreCase);
            //remove anything thats enclosed inside < >
            result = Regex.Replace(result, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);
            //replace special characters:
            result = Regex.Replace(result, @"&amp;", "&", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&nbsp;", " ", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&lt;", "<", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&gt;", ">", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"&(.{2,6});", string.Empty, RegexOptions.IgnoreCase);
            //remove extra line breaks and tabs
            result = Regex.Replace(result, @" ( )+", " ");
            result = Regex.Replace(result, "(\r)( )+(\r)", "\r\r");
            result = Regex.Replace(result, @"(\r\r)+", "\r\n");
            return result;
        }

        #endregion

        #region 微软翻译

        /// <summary>
        ///     微软翻译API :  语言类型："zh-cn", "en"
        /// </summary>
        /// <param name="orgStr">翻译原文</param>
        /// <param name="fromType">原文语言类型</param>
        /// <param name="toType">目标语言类型</param>
        /// <returns></returns>
        public static string MircsoftTanslater(string orgStr, string fromType, string toType)
        {
            string content = null;
            const string appId = "56E164FED4017D272E06AD7E16778536251CA5CB";
            string text = orgStr; // "Translate this for me";
            string from = fromType; // "en";
            string to = toType; // "zh-cn";

            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?appId=" + appId + "&text=" +
                         HttpUtility.UrlEncode(text) + "&from=" + from + "&to=" + to;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                Stream steam = response.GetResponseStream();
                if (steam != null)
                {
                    var reader = new StreamReader(steam);
                    content = reader.ReadToEnd();
                    //"<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">Hello, China</string>" 
                    content = content.Replace("<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">",
                                              "");
                    content = content.Replace("</string>", "");
                    response.Close();
                    reader.Dispose();
                }
            }
            catch (WebException e)
            {
                content = ProcessWebException(e);
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return content;
        }

        private static string ProcessWebException(WebException e)
        {
            // Obtain detailed error information
            string strResponse = null;
            using (var response = (HttpWebResponse)e.Response)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                        using (var sr = new StreamReader(responseStream, Encoding.ASCII))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                }
            }
            string result = string.Format("Http status code={0}, error message={1}", e.Status, strResponse);
            return result;
        }

        #endregion
    }

    /// <summary>
    ///     翻译返回类
    /// </summary>
    public class ResponseResult
    {
        public ResponseData ResponseData { get; set; }
        public string ResponseDetails { get; set; }
        public string ResponseStatus { get; set; }
    }

    /// <summary>
    /// </summary>
    public class ResponseData
    {
        public string TranslatedText { get; set; }
    }
}