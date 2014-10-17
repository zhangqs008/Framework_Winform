/*
 * Copyright © 2009-2012 万户网络技术有限公司
 * 文 件 名：ObjExt.cs
 * 文件描述：对象级别扩展类
 *
 * 创建标识: heyi 2012-03-01
 *
 * 修改标识：
 */

using System;
using System.IO;
using System.Xml.Serialization;

namespace Whir.Framework.Extension
{
    /// <summary>
    /// [扩展类]对象级别扩展类
    /// </summary>
    public static class ObjExtension
    {
        /// <summary>
        /// 重写ToString()的方法，该方法首先判断object是否为空，如果对象为空则返回string.Empty，否则返回该对象的字符串表现形式
        /// </summary>
        /// <param name="obj">需要转换的对象</param>
        /// <returns>字符串</returns>
        public static string ToStr(this object obj)
        {
            return obj == null ? String.Empty : obj.ToString();
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效数字表现形式，转换失败后默认返回0
        /// </summary>
        /// <param name="obj">需要转换的字符串</param>
        /// <returns>整数</returns>
        public static int ToInt(this object obj)
        {
            return ToInt32(obj, 0);
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效数字表现形式，转换失败后默认返回“0”
        /// </summary>
        /// <param name="obj">需要转换的字符串</param>
        /// <param name="defaultVal">转换失败后的默认返回值</param>
        /// <returns>整数</returns>
        public static int ToInt(this object obj, int defaultVal)
        {
            return ToInt32(obj, defaultVal);
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效数字表现形式，转换失败后默认返回“0”
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static int ToInt32(this object str)
        {
            return ToInt32(str, 0);
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效数字表现形式，转换失败后默认返回“0”
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <param name="defaultVal">转换失败后的默认返回值</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static int ToInt32(this object str, int defaultVal)
        {
            if (str == null) return defaultVal;

            int result = defaultVal;

            result = Int32.TryParse(str.ToString(), out defaultVal) ? defaultVal : result;

            return result;
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效长整型表现形式，转换失败后默认返回“0”
        /// </summary>
        /// <param name="str">需要转换的对象</param>
        /// <returns>长整型数字表现形式</returns>
        public static long ToInt64(this object str)
        {
            long defaultVal;
            if (str == null) return 0;
            Int64.TryParse(str.ToString(), out defaultVal);
            return defaultVal;
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效十进制数表现形式
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static decimal ToDecimal(this object str)
        {
            return ToDecimal(str, 0);
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效十进制数表现形式
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <param name="defaultVal">转换失败后的默认返回值</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static decimal ToDecimal(this object str, decimal defaultVal)
        {
            if (str.ToStr().IsEmpty()) return defaultVal;

            Decimal.TryParse(str.ToStr(), out defaultVal);

            return defaultVal;
        }

        /// <summary>
        /// 四舍五入格式化货币，默认保留两位小数，格式不对将返回0.00
        /// 格式前: 163.2545 格式结果: 163.25
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param> 
        /// <returns>返回转换后的数字表现形式</returns>
        public static decimal ToDecimalFormat(this object dDecimal)
        {
            return Math.Round(dDecimal.ToDecimal(), 2, MidpointRounding.AwayFromZero).ToString("F2").ToDecimal();
        }

        /// <summary>
        /// 四舍五入格式化货币，指定小数位数，格式不对将返回0.小数点位数
        /// 格式前: 163.2545 格式结果: 163.25
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        /// <param name="decimals">小数位置</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static decimal ToDecimalFormat(this string dDecimal, int decimals)
        {
            return
                Math.Round(dDecimal.ToDecimal(), decimals, MidpointRounding.AwayFromZero).ToString("F" + decimals).
                    ToDecimal();
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效数字表现形式
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static long ToLong(this object str)
        {
            return ToLong(str, 0);
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效数字表现形式
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <param name="defaultVal">转换失败后的默认返回值</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static long ToLong(this object str, long defaultVal)
        {
            if (str == null) return defaultVal;

            Int64.TryParse(str.ToString(), out defaultVal);

            return defaultVal;
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效日期时间表现形式
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static DateTime ToDateTime(this object str)
        {
            var dt = new DateTime(1900, 1, 1);

            return String.IsNullOrEmpty(str.ToStr()) ? dt : ToDateTime(str, dt);
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效日期时间表现形式
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <param name="defaultVal">转换失败后的默认返回值</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static DateTime ToDateTime(this object str, DateTime defaultVal)
        {
            if (str == null || str.ToStr().IsEmpty()) return defaultVal;

            DateTime.TryParse(str.ToString(), out defaultVal);
            return defaultVal;
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效布尔值表现形式，默认中(不区分大小写) True，1，Y 都为True类型
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static bool ToBoolean(this object str)
        {
            return ToBoolean(str, false);
        }

        /// <summary>
        /// 将此实例的字符串转换为它的有效布尔值表现形式，默认中(不区分大小写) True，1，Y 都为True类型
        /// </summary>
        /// <param name="str">需要转换的字符串</param>
        /// <param name="defaultVal">转换失败后的默认返回值</param>
        /// <returns>返回转换后的数字表现形式</returns>
        public static bool ToBoolean(this object str, bool defaultVal)
        {
            if (String.IsNullOrEmpty(str.ToStr()))
                return defaultVal;

            return str.ToString().ToLower() == "true" || str.ToString().ToLower() == "1" ||
                   str.ToString().ToLower() == "y" || str.ToString() == "是";
        }

        /// <summary>
        /// 将对象序列化为Xml串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToXml(this object obj)
        {
            try
            {
                using (var sw = new StringWriter())
                {
                    var xz = new XmlSerializer(obj.GetType());
                    xz.Serialize(sw, obj);
                    return sw.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}