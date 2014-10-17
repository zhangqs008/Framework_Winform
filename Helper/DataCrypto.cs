//--------------------------------------------------------------------------------
// 文件描述：数据加密、解密类
// 文件作者：全体开发人员
// 创建日期：2009-10-26
//--------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Whir.Framework.Helper
{
    /// <summary>
    /// 数据加密、解密类
    /// </summary>
    public static class DataCrypto
    {
        /// <summary>
        /// 对输入的Base64字符串进行解码
        /// </summary>
        /// <param name="input">输入的Base64字符串</param>
        /// <returns>解码后的字符串</returns>
        public static string Base64StringDecode(this string input)
        {
            byte[] decbuff = Convert.FromBase64String(input);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }

        /// <summary>
        /// 对输入字符串进行Base64编码
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <returns>Base64编码后的字符串</returns>
        public static string Base64StringEncode(this string input)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(encbuff);
        }

        /// <summary>
        /// 对输入字符串进行MD5加密，返回小写形式的加密字符串，字符串为32字符的十六进制格式
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string ToMD5(this string input)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(data).Replace("-", string.Empty).ToLower(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// 对输入字符串进行GB2312格式的MD5加密，返回小写形式的加密字符串，字符串为32字符的十六进制格式
        /// 此方法主要用于整合其他系统，兼容其他ASP系统的加密算法（ASP新MD5算法），与SiteWeaver的算法不完全兼容（ASP旧MD5算法）
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string MD5GB2312(this string input)
        {
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                byte[] data = md5.ComputeHash(Encoding.GetEncoding("gb2312").GetBytes(input));
                return BitConverter.ToString(data).Replace("-", string.Empty).ToLower(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// 对输入字符串进行SHA1加密，返回小写形式的加密字符串，字符串为40字符的十六进制格式
        /// </summary>
        /// <param name="input">待加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public static string Sha1(this string input)
        {
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(data).Replace("-", string.Empty).ToLower(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// 旧版中密码哈希值保存为16位，在新版中采用32位保存
        /// 将需验证的密码哈希值分别与密码明文MD5加密后的32位字符串以及密码MD5加密后从8位开始取16位的字符串进行比较
        /// 两个条件满足其一，则验证通过
        /// </summary>
        /// <param name="hashValue">需要对比的密码哈希值</param>
        /// <param name="plaintext">密码明文</param>
        /// <returns>如果验证正确，则为 true；否则为 false。</returns>
        public static bool ValidateMD5(this string hashValue, string plaintext)
        {
            string encryptedValue = plaintext.ToMD5();
            return (string.Compare(hashValue, encryptedValue, StringComparison.Ordinal) == 0) || (string.Compare(hashValue, encryptedValue.Substring(8, 16), StringComparison.Ordinal) == 0);
        }

        /// <summary>
        /// 8位密钥加密
        /// </summary>
        /// <param name="encryptValue"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string EncryptString(this string encryptValue, string sKey)
        {
            //访问数据加密标准(DES)算法的加密服务提供程序 (CSP) 版本的包装对象
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);　//建立加密对象的密钥和偏移量
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);　 //原文使用ASCIIEncoding.ASCII方法的GetBytes方法

            byte[] inputByteArray = Encoding.Default.GetBytes(encryptValue);//把字符串放到byte数组中

            MemoryStream ms = new MemoryStream();//创建其支持存储区为内存的流　
            //定义将数据流链接到加密转换的流
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            //上面已经完成了把加密后的结果放到内存中去

            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            ret.ToString();
            return ret.ToString();
        }

        /// <summary>
        /// 8位密钥3des解密
        /// </summary>
        /// <param name="encryptedValue"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string DecryptString(this string encryptedValue, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();

            byte[] inputByteArray = new byte[encryptedValue.Length / 2];
            for (int x = 0; x < encryptedValue.Length / 2; x++)
            {
                int i = (Convert.ToInt32(encryptedValue.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);　//建立加密对象的密钥和偏移量，此值重要，不能修改
            des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            //建立StringBuild对象，createDecrypt使用的是流对象，必须把解密后的文本变成流对象
            StringBuilder ret = new StringBuilder();
            return System.Text.Encoding.Default.GetString(ms.ToArray());
        }
    }
}
