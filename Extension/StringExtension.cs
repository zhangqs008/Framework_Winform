using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whir.Framework.Extension
{
    /// <summary>
    ///     [扩展类]字符串扩展类
    /// </summary>
    public static class StringExtension
    {

        /// <summary>
        /// 将指定 System.String 中的格式项替换为指定数组中相应 System.Object 实例的值的文本等效项，与 String.Format 功能效果相同
        /// </summary>
        /// <param name="str">指定的字符串</param>
        /// <param name="args">替换项的数组</param>
        public static string FormatWith(this string str, params object[] args)
        {
            //判断字符串如果为Null或空，则直接返回空值
            if (String.IsNullOrEmpty(str))
                return string.Empty;

            return String.Format(str, args);
        }

        /// <summary>
        ///     判断路径是不是图片, true 是，false 不是
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsImage(this string input)
        {
            var extesion = new[] { "jpg", "jpeg", "png", "tiff", "gif", "bmp" };
            return extesion.Any(s => input.ToLower().EndsWith("." + s));
        }
        /// <summary>
        /// 合并拼接路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        public static string Combine(this string path, string append)
        {
            return Path.Combine(path, append);
        }
        /// <summary>
        /// 移除Html
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveHtml(this string input)
        {
            var stripTags = new Regex("</?[a-z][^<>]*>", RegexOptions.IgnoreCase);
            return stripTags.Replace(input, string.Empty);
        }

        /// <summary>
        ///     判断字符串是否为空
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string input)
        {
            return string.IsNullOrEmpty(input);
        }
        /// <summary>
        /// 判断字符串不为空
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsNotEmpty(this string input)
        {
            return !string.IsNullOrEmpty(input);
        }
    }
}