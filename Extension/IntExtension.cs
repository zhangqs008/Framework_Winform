/*
 * Copyright © 2009-2012 万户网络技术有限公司
 * 文 件 名：IntExt.cs
 * 文件描述：数字扩展类
 *
 * 创建标识: heyi 2012-03-01
 *
 * 修改标识：
 */

using System;

namespace Whir.Framework.Extension
{
    /// <summary>
    /// [扩展类]数字扩展类
    /// </summary>
    public static class IntExtension
    {
        /// <summary>
        /// 将此实例的整数转换为它的有效十进制数表现形式
        /// </summary>
        /// <param name="i">需要转换的整数</param>
        /// <returns></returns>
        public static decimal ToDecimal(this int i)
        {
            return Convert.ToDecimal(i);
        }

        /// <summary>
        /// 将此实例的整数转换为它的有效双精度浮点数表现形式
        /// </summary>
        /// <param name="i">需要转换的整数</param>
        /// <returns></returns>
        public static double ToDouble(this int i)
        {
            return Convert.ToDouble(i);
        }

        /// <summary>
        /// 将此实例的整数转换为它的有效布尔值表现形式， 1为True，否则为False
        /// 该方法与 bool.ToInt()方法互相转换
        /// </summary>
        /// <param name="i">需要转换的整数</param>
        /// <returns></returns>
        public static bool ToBoolean(this int i)
        {
            return i == 1;
        }
    }
}