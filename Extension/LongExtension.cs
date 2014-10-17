/*
 * Copyright © 2009-2012 万户网络技术有限公司
 * 文 件 名：LongExt.cs
 * 文件描述：长整形数字扩展类
 *
 * 创建标识: heyi 2012-03-02
 *
 * 修改标识：
 */

using System.Net;

namespace Whir.Framework.Extension
{
    /// <summary>
    /// [扩展类]长整形数字扩展类
    /// </summary>
    public static class LongExtension
    {
        /// <summary>
        /// 把数字转换为IP地址形式的字符串
        /// </summary>
        /// <param name="l">数字形式的IP地址</param>
        public static string ToIP(this long l)
        {
            var b = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                b[3 - i] = (byte) (l >> 8*i & 255);
            }

            return new IPAddress(b).ToString();
        }
    }
}