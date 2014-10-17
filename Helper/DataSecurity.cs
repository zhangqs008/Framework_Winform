//--------------------------------------------------------------------------------
// 文件描述：数据安全转换类
// 文件作者：全体开发人员
// 创建日期：2006-07-25
//--------------------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Whir.Framework.Helper
{
    /// <summary>
    /// 数据安全转换类
    /// </summary>
    public static class DataSecurity
    {
        /// <summary>
        /// 转换为JavaScript输出
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns>转换后的字符串</returns>
        public static string ConvertToJavaScript(string value)
        {
            value = value.Replace("\\", "\\\\");
            value = value.Replace("\n", "\\n");
            value = value.Replace("\r", "\\r");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("'", "\\\'");
            return value;
        }

        /// <summary>
        /// 将字符串进行 XML 编码并返回已编码的字符串
        /// </summary>
        /// <param name="value">要编码的字符串</param>
        /// <returns>已编码的字符串</returns>
        public static string XmlEncode(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("&", "&amp;");
                value = value.Replace("<", "&lt;");
                value = value.Replace(">", "&gt;");
                value = value.Replace("'", "&apos;");
                value = value.Replace("\"", "&quot;");
            }

            return value;
        }

        /// <summary>
        /// 将对象进行 HTML 编码并返回已编码的字符串
        /// </summary>
        /// <param name="value">要编码的对象</param>
        /// <returns>已编码的字符串</returns>
        public static string HtmlEncode(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return HtmlEncode(value.ToString());
        }

        /// <summary>
        /// 将字符串进行 HTML 编码并返回已编码的字符串
        /// </summary>
        /// <param name="value">要编码的字符串</param>
        /// <returns>已编码的字符串</returns>
        public static string HtmlEncode(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("<", "&lt;");
                value = value.Replace(">", "&gt;");
                value = value.Replace(" ", "&nbsp;");
                value = value.Replace("'", "&#39;");
                value = value.Replace("\"", "&quot;");
                value = value.Replace("\r\n", "<br />");
                value = value.Replace("\n", "<br />");
            }

            return value;
        }

        /// <summary>
        /// 对 HTML 编码的对象进行解码并返回已解码的字符串
        /// </summary>
        /// <param name="value">要解码的HTML对象</param>
        /// <returns>已解码的字符串</returns>
        public static string HtmlDecode(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return HtmlDecode(value.ToString());
        }

        /// <summary>
        /// 对 HTML 编码的字符串进行解码并返回已解码的字符串
        /// </summary>
        /// <param name="value">要解码的HTML字符串</param>
        /// <returns>已解码的字符串</returns>
        public static string HtmlDecode(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                value = value.Replace("<br>", "\n");
                value = value.Replace("<br/>", "\n");
                value = value.Replace("<br />", "\n");
                value = value.Replace("&gt;", ">");
                value = value.Replace("&lt;", "<");
                value = value.Replace("&nbsp;", " ");
                value = value.Replace("&#39;", "'");
                value = value.Replace("&quot;", "\"");
            }

            return value;
        }

        /// <summary>
        /// 对字符串进行 URL 编码并返回已编码的字符串
        /// </summary>
        /// <param name="weburl">要进行 URL 编码的字符串</param>
        /// <returns>URL 编码的字符串</returns>
        public static string UrlEncode(string weburl)
        {
            if (string.IsNullOrEmpty(weburl))
            {
                return null;
            }

            return Regex.Replace(weburl, "[^a-zA-Z0-9,_&=?%\\.:/-]+", new MatchEvaluator(UrlEncodeMatch));
        }

        /// <summary>
        /// 对字符串进行 URL 编码并返回已编码的字符串，通过参数控制是否使用.net自带的方法
        /// </summary>
        /// <param name="weburl">要进行 URL 编码的字符串</param>
        /// <param name="systemEncode">如果为 true，则使用.net自带的方法</param>
        /// <returns>URL 编码的字符串</returns>
        public static string UrlEncode(string weburl, bool systemEncode)
        {
            if (string.IsNullOrEmpty(weburl))
            {
                return null;
            }

            if (systemEncode)
            {
                return HttpUtility.UrlEncode(weburl);
            }
            else
            {
                return UrlEncode(weburl);
            }
        }

        /// <summary>
        /// 过滤掉字符串中会引起注入攻击的字符
        /// </summary>
        /// <param name="strchar">要过滤的字符串</param>
        /// <returns>已过滤的字符串</returns>
        public static string FilterBadChar(string strchar)
        {
            string tempstrChar;
            string newstrChar = string.Empty;
            if (string.IsNullOrEmpty(strchar))
            {
                newstrChar = string.Empty;
            }
            else
            {
                tempstrChar = strchar;
                string[] strBadChar = { "+", "'", "%", "^", "&", "?", "(", ")", "<", ">", "[", "]", "{", "}", "/", "\"", ";", ":", Convert.ToChar(0).ToString(), "--" };
                for (int i = 0; i < strBadChar.Length; i++)
                {
                    tempstrChar = tempstrChar.Replace(strBadChar[i], string.Empty);
                }

                newstrChar = Regex.Replace(tempstrChar, "@+", "@");
            }

            return newstrChar;
        }

        /// <summary>
        /// 过滤跨站点脚本中的危险字符串
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <returns>过滤后的字符串</returns>
        public static string RemoveXss(this string input)
        {
            string str;
            //// 循环解码
            do
            {
                str = input;
                //// 替换掉HTML字符实体(Character Entities)名字和分号之间的空白字符，比如：&auml    ;替换成&auml;
                input = Regex.Replace(input, @"(&#*\w+)[\x00-\x20]+;", "$1;");

                //// 将无分号结束符的数字编码实体规范成带分号的标准形式
                input = Regex.Replace(input, @"(&#x*[0-9A-F]+);*", "$1;", RegexOptions.IgnoreCase);

                // 将&nbsp; &lt; &gt; &amp; &quot;字符实体中的 & 替换成 &amp; 以便在进行HtmlDecode时保留这些字符实体
                input = Regex.Replace(input, @"&(amp|lt|gt|nbsp|quot);", "&amp;$1;");

                // 将HTML字符实体进行解码，以消除编码字符对后续过滤的影响
                input = HttpUtility.HtmlDecode(input);
            }
            while (str != input);

            // 下面三行为别把 '' " "  之间的 > 转成 &gt;
            input = Regex.Replace(input, @"(?<=(<[\s\S]*=\s*""[^""]*))>(?=([^""]*""[\s\S]*>))", "&gt;", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"(?<=(<[\s\S]*=\s*'[^']*))>(?=([^']*'[\s\S]*>))", "&gt;", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"(?<=(<[\s\S]*=\s*`[^`]*))>(?=([^`]*`[\s\S]*>))", "&gt;", RegexOptions.IgnoreCase);

            do
            {
                str = input;   // 替换掉style中的 "\" 
                input = Regex.Replace(input, @"(<[^>]+?style[\x00-\x20]*=[\x00-\x20]*[^>]*?)\\([^>]*>)", "$1/$2", RegexOptions.IgnoreCase);
            }
            while (str != input);

            // 将ASCII码表中前32个字符中的非打印字符替换成空字符串，保留 9、10、13、32，它们分别代表 制表符、换行符、回车符和空格。
            input = Regex.Replace(input, @"[\x00-\x08\x0b-\x0c\x0e-\x19]", string.Empty);

            // 替换以on和xmlns开头的属性
            input = Regex.Replace(input, @"(<[^>]+?[\x00-\x20""'/])(on|xmlns)[^>]*>", @"$1>", RegexOptions.IgnoreCase);

            // 替换javascript
            input = Regex.Replace(input, @"([a-z]*)[\x00-\x20]*=[\x00-\x20]*([`'""]*)[\x00-\x20]*j[\x00-\x20]*a[\x00-\x20]*v[\x00-\x20]*a[\x00-\x20]*s[\x00-\x20]*c[\x00-\x20]*r[\x00-\x20]*i[\x00-\x20]*p[\x00-\x20]*t[\x00-\x20]*:", "$1=$2nojavascript...", RegexOptions.IgnoreCase);

            // 替换vbscript
            input = Regex.Replace(input, @"([a-z]*)[\x00-\x20]*=[\x00-\x20]*([`'""]*)[\x00-\x20]*v[\x00-\x20]*b[\x00-\x20]*s[\x00-\x20]*c[\x00-\x20]*r[\x00-\x20]*i[\x00-\x20]*p[\x00-\x20]*t[\x00-\x20]*:", "$1=$2novbscript...", RegexOptions.IgnoreCase);

            // 替换style中的注释部分，比如：<div style="xss:expres/*comment*/sion(alert('xss'))">
            input = Regex.Replace(input, @"(<[^>]+style[\x00-\x20]*=[\x00-\x20]*[^>]*?)/\*[^>]*\*/([^>]*>)", @"$1$2", RegexOptions.IgnoreCase);

            // 替换expression
            input = Regex.Replace(input, @"(<[^>]+?)style[\x00-\x20]*=[\x00-\x20]*([`'""]*).*?[eｅＥ][xｘＸ][pｐＰ][rｒＲ][eｅＥ][sｓＳ][sｓＳ][iｉＩ][oｏＯ][nｎＮ][\x00-\x20]*[\(\（][^>]*>", @"$1>", RegexOptions.IgnoreCase);

            // 替换behaviour
            input = Regex.Replace(input, @"(<[^>]+?)style[\x00-\x20]*=[\x00-\x20]*([`'""]*).*?behaviour[^>]*>", @"$1>", RegexOptions.IgnoreCase);

            // 替换behavior
            input = Regex.Replace(input, @"(<[^>]+?)style[\x00-\x20]*=[\x00-\x20]*([`'""]*).*?behavior[^>]*>", @"$1>", RegexOptions.IgnoreCase);

            // 替换script
            input = Regex.Replace(input, @"(<[^>]+?)style[\x00-\x20]*=[\x00-\x20]*([`'""]*).*?s[\x00-\x20]*c[\x00-\x20]*r[\x00-\x20]*i[\x00-\x20]*p[\x00-\x20]*t[\x00-\x20]*:*[^>]*>", @"$1>", RegexOptions.IgnoreCase);

            // 替换namespaced elements 不需要
            input = Regex.Replace(input, @"</*\w+:\w[^>]*>", "　");

            string oldhtmlString;
            do
            {
                oldhtmlString = input;
                input = Regex.Replace(input, @"</*(applet|meta|xml|blink|link|style|script|embed|object|iframe|frame|frameset|ilayer|layer|bgsound|title|base)[^>]*>?", "　", RegexOptions.IgnoreCase);
            }
            while (oldhtmlString != input);

            // 防止生成shtml时调用 <!-- #include file="../web.config" --> 等
            input = Regex.Replace(input, @"<!--([\s\S]*?)-->", "&lt;!--$1--&gt;");

            // 防止注译语句有没有闭合
            input = input.Replace("<!--", "&lt;!--");

            return input;
        }

        /// <summary>
        /// UrlEncode匹配
        /// </summary>
        /// <param name="match">匹配后得到的字符串</param>
        /// <returns>字符的URL实体</returns>
        private static string UrlEncodeMatch(Match match)
        {
            string matchString = match.ToString();
            if (matchString.Length < 1)
            {
                return matchString;
            }

            StringBuilder sb = new StringBuilder();

            foreach (char ii in matchString)
            {
                if (ii > '\x007f')
                {
                    sb.Append(HttpUtility.UrlEncode(ii.ToString(), Encoding.UTF8));
                }
                else
                {
                    sb.AppendFormat("%{0:X2}", (int)ii);
                }
            }

            return sb.ToString();
        }
    }
}
