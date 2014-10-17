//--------------------------------------------------------------------------------
// 文件描述：数据验证类
// 文件作者：全体开发人员
// 创建日期：2006-09-11
//--------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace Whir.Framework 
{
    /// <summary>
    /// 数据验证类 
    /// </summary>
    public static class DataValidator
    {
        /// <summary>
        /// 数据验证类使用的正则表述式选项
        /// </summary>
        private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        /// <summary>
        /// 检测字符串是否为数字捕获正则
        /// </summary>
        private static readonly Regex NumberRegex = new Regex(@"^[0-9]+$", Options);

        /// <summary>
        /// 检测字符串是否为数字（可带正负号）捕获正则
        /// </summary>
        private static readonly Regex NumberSignRegex = new Regex(@"^[+-]?[0-9]+$", Options);

        /// <summary>
        /// 检测字符串是否为浮点数捕获正则
        /// </summary>
        private static readonly Regex DecimalRegex = new Regex(@"^[0-9]+(\.[0-9]+)?$", Options);

        /// <summary>
        /// 检测字符串是否是有效的IP地址捕获正则
        /// </summary>
        private static readonly Regex IPRegex = new Regex(@"^(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])\.(\d{1,2}|1\d\d|2[0-4]\d|25[0-5])$", Options);

        /// <summary>
        /// 检测字符串是否为有效的URL地址捕获正则
        /// </summary>
        private static readonly Regex UrlRegex = new Regex(@"^http(s)?://(([\w-]+\.)+[\w-]+|localhost)(:\d{1,5})?(/[\w- ./?%&=]*)?", Options);

        /// <summary>
        /// 检测字符串是否为有效的邮件地址捕获正则
        /// </summary>
        private static readonly Regex EmailRegex = new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", Options);

        /// <summary>
        /// 检测字符串是否为有效的邮政编码捕获正则
        /// </summary>
        private static readonly Regex PostCodeRegex = new Regex(@"^\d{6}$", Options);

        /// <summary>
        /// 检测字符串是否为有效的ID捕获正则
        /// </summary>
        private static readonly Regex ValidIdRegex = new Regex(@"^[\d\|\s,-]*$", Options);

        /// <summary>
        /// 检测字符串是否为数字
        /// </summary>
        /// <param name="input">需要检查的字符串</param>
        /// <returns>如果字符串为数字，则为 true；否则为 false。</returns>
        public static bool IsNumber(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            else
            {
                return NumberRegex.IsMatch(input);
            }
        }

        /// <summary>
        /// 检测字符串是否为数字，可带正负号
        /// </summary>
        /// <param name="input">需要检查的字符串</param>
        /// <returns>如果字符串为数字，则为 true；否则为 false。</returns>
        public static bool IsNumberSign(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            else
            {
                return NumberSignRegex.IsMatch(input);
            }
        }

        /// <summary>
        /// 检测字符串是否为浮点数
        /// </summary>
        /// <param name="input">需要检查的字符串</param>
        /// <returns>如果字符串为浮点数，则为 true；否则为 false。</returns>
        public static bool IsDecimal(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            else
            {
                return DecimalRegex.IsMatch(input);
            }
        }

        /// <summary>
        /// 判断字符串是否是有效的IP地址
        /// </summary>
        /// <param name="input">IP地址字符串</param>
        /// <returns>有效IP地址返回true ；否则返回false</returns>
        public static bool IsIP(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return IPRegex.IsMatch(input);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检测字符串是否为有效的URL地址
        /// </summary>
        /// <param name="input">需要检查的字符串</param>
        /// <returns>如果字符串为有效的URL地址，则为 true；否则为 false。</returns>
        public static bool IsUrl(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            else
            {
                return UrlRegex.IsMatch(input);
            }
        }

        /// <summary>
        /// 检测字符串是否为有效的邮件地址
        /// </summary>
        /// <param name="input">需要检查的字符串</param>
        /// <returns>如果字符串为有效的邮件地址，则为 true；否则为 false。</returns>
        public static bool IsEmail(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            else
            {
                return EmailRegex.IsMatch(input);
            }
        }

        /// <summary>
        /// 检测字符串是否为有效的邮政编码
        /// </summary>
        /// <param name="input">需要检查的字符串</param>
        /// <returns>如果字符串为有效的邮政编码，则为 true；否则为 false。</returns>
        public static bool IsPostCode(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            else
            {
                return PostCodeRegex.IsMatch(input);
            }
        }

        /// <summary>
        /// 检测字符串是否为有效的ID
        /// </summary>
        /// <param name="input">需要检查的字符串</param>
        /// <returns>如果字符串为有效的ID，则为 true；否则为 false。</returns>
        public static bool IsValidId(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            else
            {
                return ValidIdRegex.IsMatch(input);
            }
        }
    }
}
