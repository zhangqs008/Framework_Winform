/*
 * Copyright © 2009-2012 万户网络技术有限公司
 * 文 件 名：DateTimeExt.cs
 * 文件描述：日期时间扩展类
 *
 * 创建标识: heyi 2012-03-01
 *
 * 修改标识：
 */

using System;

namespace Whir.Framework.Extension
{
    /// <summary>
    /// [扩展类]日期时间扩展类
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// 获取MS SQL数据支持的最少日期
        /// </summary>
        public static DateTime MinDateTime
        {
            get { return new DateTime(1900, 1, 1); }
        }

        /// <summary>
        /// 格式化日期(24小时制)，返回格式如：2012-03-02
        /// </summary>
        /// <param name="dt">需要格式的日期</param>
        /// <returns>格式化后的日期</returns>
        public static string Format(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 格式化日期(24小时制)，返回格式如：2012-03-02 09:10:11
        /// </summary>
        /// <param name="dt">需要格式的日期</param>
        /// <returns>格式化后的日期</returns>
        public static string FormatL(this DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 格式化日期(24小时制)，返回格式如：2012年03月02日
        /// </summary>
        /// <param name="dt">需要格式的日期</param>
        /// <returns>格式化后的日期</returns>
        public static string FormatD(this DateTime dt)
        {
            return dt.ToString("yyyy年MM月dd日");
        }

        /// <summary>
        /// 格式化日期(24小时制)，返回格式如：2012年03月02日 09时10分11秒
        /// </summary>
        /// <param name="dt">需要格式的日期</param>
        /// <returns>格式化后的日期</returns>
        public static string FormatLD(this DateTime dt)
        {
            return dt.ToString("yyyy年MM月dd日 HH时mm分ss秒");
        }
    }
}