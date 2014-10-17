using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Whir.Framework
{
    /// <summary>
    ///     网页采集辅助类
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        ///     取得字符里的Dom元素 不包含元素属性
        /// </summary>
        /// <param name="source"></param>
        /// <param name="domElem"></param>
        /// <returns></returns>
        public static List<string> GetDomElem(string source, string domElem)
        {
            var matchList = new List<string>();
            string regStr = string.Format("<{0}[^>]*?>[\\s\\S]+?<\\/{0}>", domElem);
            try
            {
                var regex = new Regex(regStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(source);
                foreach (Match match in matches)
                {
                    matchList.Add(match.Value);
                }
            }
            catch (Exception ex)
            {
                matchList.Add(ex.Message);
            }
            return matchList;
        }

        /// <summary>
        ///     取得字符里的Dom元素 包含元素属性 如：class="aa"
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tagName"></param>
        /// <param name="tagValue"></param>
        /// <returns></returns>
        public static List<string> GetDomElemByAttr(string source, string tagName, string tagValue)
        {
            var matchList = new List<string>();
            string regStr =
                string.Format(
                    @"<(?<HtmlTag>[\w]+)[^>]*\s{0}[\s]*?=[\s]*?(?<Quote>[""']?){1}(?(Quote)\k<Quote>)[""']?[^>]*>((?<Nested><\k<HtmlTag>[^>]*>)|</\k<HtmlTag>>(?<-Nested>)|[\s\S]*?)*</\k<HtmlTag>>",
                    tagName.ToLower(), tagValue);
            try
            {
                var regex = new Regex(regStr, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = regex.Matches(source);
                foreach (Match match in matches)
                {
                    matchList.Add(match.Value);
                }
            }
            catch (Exception ex)
            {
                matchList.Add(ex.Message);
            }
            return matchList;
        }

        /// <summary>
        ///     取得字符里的A元素键值对  [name，url]
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetDomElem_A(string source)
        {
            var matchList = new Dictionary<string, string>();
            const string pattern = "<a[^>]*? href=[\"'](?<url>[^\"']*?)[\"'][^>]*?>(?<text>[\\w\\W]*?)</a>";
            try
            {
                var regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = regex.Matches(source);

                foreach (Match match in matches)
                {
                    string key = RemoveHtml(match.Value);
                    if (!matchList.ContainsKey(key))
                    {
                        matchList.Add(key, GetUrlArray(match.Value)[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                matchList.Add(ex.Message, "");
            }
            return matchList;
        }



        /// <summary>
        ///     获取页面内容后，用匹配url正则表达式抓取内容中的url
        /// </summary>
        /// <param name="code">列表代码</param>
        /// <returns>返回截取后的URL地址</returns>
        public static List<string> GetUrlArray(string code)
        {
            var urlList = new List<string>();
            var regex =
                new Regex(@"(http://)?[\w-\.]*([\/]?[\w-])+[\w-]*\.(htm|html|shtm|shtml|aspx|asp|php|jsp)+[\w-\=\?]*",
                          RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(code);
            foreach (Match match in matches)
            {
                urlList.Add(match.Value);
            }
            return urlList;
        }

        /// <summary>
        ///     获取内容code中所有都图片地址
        /// </summary>
        /// <returns>返回截取后都图片地址</returns>
        public static Dictionary<string, string> GetImgUrlArray(string content)
        {
            var imgList = new Dictionary<string, string>();
            var reg = new Regex(@"<img[\s\S]*?src=(""(?<src>[^']*?)""|'(?<src>[^']*?)'|(?<src>[^>\s]*))[^>]*?>(.*?)");
            MatchCollection m = reg.Matches(content.ToLower());
            foreach (Match match in m)
            {
                string matchValue = match.Groups["src"].Value;
                if (!imgList.ContainsKey(matchValue))
                {
                    imgList.Add(matchValue, matchValue);
                }
            }
            return imgList;
        }

        /// <summary>
        ///     将相对地址转换为绝对地址
        /// </summary>
        /// <param name="relativeAddress">要转换的相对地址</param>
        /// <param name="absoluteAddress">当前网页地址</param>
        /// <returns>返回转换后的地址</returns>
        public static string ConvertToAbsluteUrl(string relativeAddress, string absoluteAddress)
        {
            if (string.IsNullOrEmpty(relativeAddress))
            {
                return string.Empty;
            }
            if (relativeAddress.Contains("://"))
            {
                return relativeAddress;
            }
            if (string.IsNullOrEmpty(absoluteAddress))
            {
                return string.Empty;
            }
            if (!absoluteAddress.Contains("://"))
            {
                return string.Empty;
            }
            var baseUrl = new Uri(absoluteAddress);
            var webrul = new Uri(baseUrl, relativeAddress);
            return webrul.ToString();
        }

        /// <summary>
        ///     替换所有HTML标签为空
        /// </summary>
        /// <param name="input">The string whose values should be replaced.</param>
        /// <returns>A string.</returns>
        public static string RemoveHtml(string input)
        {
            var stripTags = new Regex("</?[a-z][^<>]*>", RegexOptions.IgnoreCase);
            return stripTags.Replace(input, string.Empty);
        }

        /// <summary>
        ///     移除字符串中的空格及换行符
        /// </summary>
        /// <param name="input">The string whose values should be replaced.</param>
        /// <returns>A string.</returns>
        public static string RemoveBlank(string input)
        {
            input = input.Replace("\r", string.Empty);
            input = input.Replace("\n", string.Empty);
            input = input.Replace(" ", string.Empty);
            input = input.Replace("&nbsp;", string.Empty);
            return input;
        }

        /// <summary>
        ///  获取网页的HTML内容，根据网页的charset自动判断Encoding 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns> 
        public static string GetHtml(string url)
        {
            return GetHtml(url, null);
        }

        // 获取网页的HTML内容，指定Encoding  
        private static string GetHtml(string url, Encoding encoding)
        {
            string getSource;
            try
            {
                byte[] buf = new WebClient().DownloadData(url);
                if (encoding != null) return encoding.GetString(buf);
                string html = Encoding.UTF8.GetString(buf);
                encoding = GetEncoding(html);
                if (encoding == null || (Equals(encoding, Encoding.UTF8))) return html;
                getSource = encoding.GetString(buf);
            }
            catch (NotSupportedException exception)
            {
                getSource = exception.Message;
            }
            catch (InvalidOperationException exception)
            {
                getSource = exception.Message;
            }
            catch (IOException exception)
            {
                getSource = exception.Message;
            }
            return getSource;
        }

        /// <summary>
        /// 根据网页的HTML内容提取网页的Encoding
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns> 
        private static Encoding GetEncoding(string html)
        {
            const string pattern = @"(?i)\bcharset=(?<charset>[-a-zA-Z_0-9]+)";
            string charset = Regex.Match(html, pattern).Groups["charset"].Value;
            try
            {
                return Encoding.GetEncoding(charset);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}