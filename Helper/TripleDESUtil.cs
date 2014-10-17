 
using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace Whir.Framework.Helper
{
    public sealed class TripleDESUtil
    {
        private const string DEFAULT_KEY = "abET34Gd#%%#hhj5$6gf7EA1";
        //最大192位加密，24字节
        private static TripleDESService m_TripleDES = new TripleDESService(DEFAULT_KEY);

         
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Encrypt(string source)
        {
            return m_TripleDES.Encrypt(source);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Decrypt(string source)
        {
            return m_TripleDES.Decrypt(source);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Encrypt(string key, string source)
        {
            return new TripleDESService(key).Encrypt(source);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="key"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Decrypt(string key, string source)
        {
            return new TripleDESService(key).Decrypt(source);
        }
    }
    /// <summary>
    /// 三重DES
    /// </summary>
    public class TripleDESService : IDisposable
    {
        private TripleDES mydes;
        private string key;
        //private string iv = "#$^%&&*Yisifhsfjsljfslhgosdshf26382837sdfjskhf97(*&(*";
        private string iv = "#$^%&&*Y"; //8字节

        /// <summary>
        /// 对称加密类的构造函数
        /// </summary>
        /// <param name="key"></param>
        public TripleDESService(string key)
        {
            mydes = new TripleDESCryptoServiceProvider();
            this.key = key;
            mydes.Key = GetLegalKey();
            mydes.IV = GetLegalIV();
        }

        /// <summary>
        /// 对称加密类的构造函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public TripleDESService(string key, string iv)
            : this(key)
        {
            this.iv = iv;
        }

        /// <summary>
        /// 获得密钥
        /// </summary>
        /// <returns>密钥</returns>
        private byte[] GetLegalKey()
        {
            //mydes.GenerateKey();
            //byte[] bytTemp = mydes.Key;
            //int KeyLength = bytTemp.Length;
            const int keyLength = 24;
            byte[] bs = new byte[keyLength];
            int len = this.key.Length;
            if (len == keyLength)
            {
                bs = ASCIIEncoding.ASCII.GetBytes(this.key);
            }
            else if (len > keyLength)
            {
                string temp = this.key.Substring(0, keyLength);
                bs = ASCIIEncoding.ASCII.GetBytes(temp);
            }
            else
            {
                string temp = this.key.PadRight(keyLength, ' ');
                bs = ASCIIEncoding.ASCII.GetBytes(temp);
            }
            if (TripleDES.IsWeakKey(bs))
            {
                throw new CryptographicException(String.Format("指定密钥{0}是TripleDES的已知弱密钥，不能使用。", this.key));
            }
            return bs;
        }

        /// <summary>
        /// 获得初始向量IV
        /// </summary>
        /// <returns>初试向量IV</returns>
        private byte[] GetLegalIV()
        {
            //mydes.GenerateIV();
            //byte[] bytTemp = mydes.IV;
            //int IVLength = bytTemp.Length;
            int ivLength = 8;
            int len = this.iv.Length;
            if (len == ivLength)
            {
                return ASCIIEncoding.ASCII.GetBytes(this.iv);
            }

            string sTemp = String.Empty;
            if (len > ivLength)
            {
                sTemp = this.iv.Substring(0, ivLength);
            }
            else
            {
                sTemp = this.iv.PadRight(ivLength, ' ');
            }
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }

        /// <summary>
        /// 加密方法
        /// </summary>
        /// <param name="input">待加密的串</param>
        /// <returns>经过加密的串</returns>
        public string Encrypt(string input)
        {
            try
            {
                byte[] bytIn = UTF8Encoding.UTF8.GetBytes(input);
                MemoryStream ms = new MemoryStream();
                //mydes.Key = GetLegalKey();
                //mydes.IV = GetLegalIV();
                ICryptoTransform encrypto = mydes.CreateEncryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                ms.Close();
                byte[] bytOut = ms.ToArray();
                return Convert.ToBase64String(bytOut);
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException(String.Format("在文件解密的时候出现错误！key：{0} 待解密的串：{1} 错误提示： {2}\n", this.key, input, ex.Message));
            }
            catch (Exception ex)
            {
                throw new Exception("在文件加密的时候出现错误！错误提示： \n" + ex.Message);
            }
        }

        /// <summary>
        /// 解密方法
        /// </summary>
        /// <param name="input">待解密的串</param>
        /// <returns>经过解密的串</returns>
        public string Decrypt(string input)
        {
            try
            {
                byte[] bytIn = Convert.FromBase64String(input);
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
                ICryptoTransform encrypto = mydes.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException(String.Format("在文件解密的时候出现错误！key：{0} 待解密的串：{1} 错误提示： {2}\n", this.key, input, ex.Message));
            }
            catch (Exception ex)
            {
                throw new Exception("在文件解密的时候出现错误！错误提示： \n" + ex.Message);
            }
        }

        /// <summary>
        /// 加密方法byte[] to byte[]
        /// </summary>
        /// <param name="input">待加密的byte数组</param>
        /// <returns>经过加密的byte数组</returns>
        public byte[] Encrypt(byte[] input)
        {
            try
            {
                byte[] bytIn = input;
                MemoryStream ms = new MemoryStream();
                ICryptoTransform encrypto = mydes.CreateEncryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                ms.Close();
                byte[] bytOut = ms.ToArray();
                return bytOut;
            }
            catch (Exception ex)
            {
                throw new Exception("在文件加密的时候出现错误！错误提示： \n" + ex.Message);
            }
        }

        /// <summary>
        /// 解密方法byte[] to byte[]
        /// </summary>
        /// <param name="input">待解密的byte数组</param>
        /// <returns>经过解密的byte数组</returns>
        public byte[] Decrypt(byte[] input)
        {
            try
            {
                byte[] bytIn = input;
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
                ICryptoTransform encrypto = mydes.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs);
                return UTF8Encoding.UTF8.GetBytes(sr.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw new Exception("在文件解密的时候出现错误！错误提示： \n" + ex.Message);
            }
        }

        /// <summary>
        /// 加密方法File to File
        /// </summary>
        /// <param name="inFileName">待加密文件的路径</param>
        /// <param name="outFileName">待加密后文件的输出路径</param>

        public void Encrypt(string inFileName, string outFileName)
        {
            try
            {

                FileStream fin = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(outFileName, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);

                byte[] bin = new byte[100];
                long rdlen = 0;
                long totlen = fin.Length;
                int len;

                ICryptoTransform encrypto = mydes.CreateEncryptor();
                CryptoStream cs = new CryptoStream(fout, encrypto, CryptoStreamMode.Write);
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    cs.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }
                cs.Close();
                fout.Close();
                fin.Close();

            }
            catch (Exception ex)
            {
                throw new Exception("在文件加密的时候出现错误！错误提示： \n" + ex.Message);
            }
        }

        /// <summary>
        /// 解密方法File to File
        /// </summary>
        /// <param name="inFileName">待解密文件的路径</param>
        /// <param name="outFileName">待解密后文件的输出路径</param>
        public void Decrypt(string inFileName, string outFileName)
        {
            try
            {
                FileStream fin = new FileStream(inFileName, FileMode.Open, FileAccess.Read);
                FileStream fout = new FileStream(outFileName, FileMode.OpenOrCreate, FileAccess.Write);
                fout.SetLength(0);

                byte[] bin = new byte[100];
                long rdlen = 0;
                long totlen = fin.Length;
                int len;
                ICryptoTransform encrypto = mydes.CreateDecryptor();
                CryptoStream cs = new CryptoStream(fout, encrypto, CryptoStreamMode.Write);
                while (rdlen < totlen)
                {
                    len = fin.Read(bin, 0, 100);
                    cs.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }
                cs.Close();
                fout.Close();
                fin.Close();

            }
            catch (Exception ex)
            {
                throw new Exception("在文件解密的时候出现错误！错误提示： \n" + ex.Message);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            //;
        }

        #endregion
    }
}
