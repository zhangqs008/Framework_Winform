/*
 * Copyright © 2009-2012 万户网络技术有限公司
 * 文 件 名：DecimalExt.cs
 * 文件描述：十进制数字对象扩展类
 *
 * 创建标识: heyi 2012-03-01
 *
 * 修改标识：
 */

using System;

namespace Whir.Framework.Extension
{
    /// <summary>
    /// [扩展类]十进制数字对象扩展类
    /// </summary>
    public static class DecimalExtension
    {
        /// <summary>
        /// 把数字转换为大写形式(只能精确到小数点后两位数，即到分)
        /// </summary>
        /// <param name="num">数字</param>
        /// <returns>返回转换后的大写形式</returns>
        public static string ToCmycurD(this decimal num)
        {
            string str1 = "零壹贰叁肆伍陆柒捌玖"; //0-9所对应的汉字
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; //数字位所对应的汉字
            string str5 = ""; //人民币大写金额形式
            int i; //循环变量
            string ch2 = ""; //数字位的汉字读法
            int nzero = 0; //用来计算连续的零值是几个

            num = Math.Round(Math.Abs(num), 2); //将num取绝对值并四舍五入取2位小数
            string str4 = ((long) (num*100)).ToString();
            int j = str4.Length;
            if (j > 15)
            {
                return "溢出";
            }
            str2 = str2.Substring(15 - j); //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分

            //循环取出每一位需要转换的值
            for (i = 0; i < j; i++)
            {
                string str3 = str4.Substring(i, 1); //从原num值中取出的值
                int temp = Convert.ToInt32(str3); //从原num值中取出的值
                string ch1; //数字的汉语读法
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp*1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp*1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp*1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp*1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整”
                    str5 = str5 + '整';
                }
            }
            if (num == 0)
            {
                str5 = "零元整";
            }
            return str5;
        }

        /// <summary>
        /// 四舍五入格式化货币，默认保留小数点后两位小数，如为整数保留.00的格式
        /// 格式前: 163.2545 格式结果: 163.25
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        public static decimal Format(this decimal dDecimal)
        {
            return Math.Round(dDecimal, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 四舍五入格式化货币，如为整数也保留格式
        /// 格式前: 163.2545 格式结果: 163.25
        /// </summary>
        /// <example>
        /// <para>decimal d = 163.2545m;</para>
        /// <para>Console.WriteLine(d.Format(3));</para>
        /// <para>显示结果为: 163.255</para>
        /// </example>
        /// <param name="dDecimal">需要转换的货币数字</param>
        /// <param name="decimals">需要保留的小数位，如需保留3位填写: 3</param>
        public static decimal Format(this decimal dDecimal, int decimals)
        {
            return Math.Round(dDecimal, decimals, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// 利用科学记数法格式化货币，返回格式： 100,000.00
        /// 格式前: 123456.78 格式结果: 1,234,56.78
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        public static string FormatN(this decimal dDecimal)
        {
            return FormatN(dDecimal, 2);
        }

        /// <summary>
        /// 四舍五入格式化货币,然后自定义保留小数位数，最后利用科学记数法显示
        /// 格式前: 123456.78 格式结果: 1,234,56.78
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        /// <param name="decimals">需要保留的小数位，如需保留3位填写: 3</param>
        public static string FormatN(this decimal dDecimal, int decimals)
        {
            string specifier = String.Format("N{0}", decimals);

            return dDecimal.Format(decimals).ToString(specifier);
        }

        /// <summary>
        /// 将指定的货币数字格式化，默认为四舍五入保留2位小数
        /// 格式前: 163.25 格式结果: ￥163.25
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        /// <returns>返回转换后的货币数字字符串</returns>
        public static string FormatC(this decimal dDecimal)
        {
            return FormatC(dDecimal, 2);
        }

        /// <summary>
        /// 将指定的货币数字格式化，默认为四舍五入
        /// 格式前: 163.25 格式结果: ￥163.25
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        /// /// <param name="decimals">需要保留的小数位，如需保留3位填写: 3</param>
        /// <returns>返回转换后的货币数字字符串</returns>
        public static string FormatC(this decimal dDecimal, int decimals)
        {
            string specifier = String.Format("C{0}", decimals);

            return dDecimal.ToString(specifier);
        }

        /// <summary>
        /// 将指定的货币数字格式化为百分比，默认为四舍五入保留2位小数
        /// 格式前: 0.16325 格式结果: 16.325%
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        /// <returns>返回转换后的货币数字字符串</returns>
        public static string FormatP(this decimal dDecimal)
        {
            return FormatP(dDecimal, 2);
        }

        /// <summary>
        /// 将指定的货币数字格式化为百分比，默认为四舍五入保留2位小数
        /// 格式前: 0.16325 格式结果: 16.325%
        /// </summary>
        /// <param name="dDecimal">需要转换的货币数字</param>
        /// <param name="decimals">需要保留的小数位，如需保留3位填写: 3</param>
        /// <returns>返回转换后的货币数字字符串</returns>
        public static string FormatP(this decimal dDecimal, int decimals)
        {
            string specifier = String.Format("P{0}", decimals);

            return (dDecimal).ToString(specifier);
        }
    }
}