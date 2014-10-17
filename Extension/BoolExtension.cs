/*
 * Copyright © 2009-2012 万户网络技术有限公司
 * 文 件 名：BoolExt.cs
 * 文件描述：布尔类型对象的扩展类
 *
 * 创建标识: heyi 2012-03-01
 *
 * 修改标识：
 */

namespace Whir.Framework.Extension
{
    /// <summary>
    /// [扩展类]布尔类型对象的扩展类
    /// </summary>
    public static class BoolExtension
    {
        /// <summary>
        /// 将此实例的布尔值转换为它的有效数字表现形式，True的转换形式为“1”，False为“0”
        /// </summary>
        /// <param name="bol">需要转换的布尔值</param>
        /// <returns></returns>
        public static int ToInt(this bool bol)
        {
            return bol ? 1 : 0;
        }

        /// <summary>
        /// 将此实例的布尔值转换为它的有效字符串表现形式，True的转换形式为“Y”，False为“N”
        /// </summary>
        /// <param name="bol"></param>
        /// <returns></returns>
        public static string ToString(this bool bol)
        {
            return bol ? "Y" : "N";
        }
    }
}