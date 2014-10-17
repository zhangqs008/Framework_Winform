//--------------------------------------------------------------------------------
// 文件描述：数据格式化器
// 文件作者：张清山
// 创建日期：2013-12-10 15:09:32
// 修改记录： 
//--------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Whir.Framework.Extension
{
    /// <summary>
    ///    序列化器
    /// </summary>
    public class Serializer
    {
        /// <summary>
        ///     Xml序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns></returns>
        public static string XmlSerialize<T>(T t)
        {
            using (var sw = new StringWriter())
            {
                var xz = new XmlSerializer(t.GetType());
                xz.Serialize(sw, t);
                return sw.ToString();
            }
        }

        /// <summary>
        /// 使用XmlSerializer序列化对象(可以不包括命名空间申明，即<?xml version="1.0" encoding="utf-16"?>)
        /// </summary>
        /// <typeparam name="T">需要序列化的对象类型，必须声明[Serializable]特征</typeparam>
        /// <param name="obj">需要序列化的对象</param>
        /// <param name="omitXmlDeclaration">true:省略XML声明;否则为false.默认false，即编写 XML 声明。</param>
        /// <returns>序列化后的字符串</returns>
        public static string XmlSerialize<T>(T obj, bool omitXmlDeclaration)
        {
            var stream = new MemoryStream();
            XmlWriter xmlwriter = XmlWriter.Create(stream,
                                                   new XmlWriterSettings
                                                       {
                                                           OmitXmlDeclaration = omitXmlDeclaration,
                                                           Encoding = new UTF8Encoding(false)
                                                       });
            //这里如果直接写成：Encoding = Encoding.UTF8 会在生成的xml中加入BOM(Byte-order Mark) 信息(Unicode 字节顺序标记) ， 所以new System.Text.UTF8Encoding(false)是最佳方式，省得再做替换的麻烦
            var xmlns = new XmlSerializerNamespaces();
            xmlns.Add(String.Empty, String.Empty);
            var ser = new XmlSerializer(typeof(T));
            ser.Serialize(xmlwriter, obj, xmlns);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// 使用XmlSerializer序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">文件路径</param>
        /// <param name="obj">需要序列化的对象</param>
        /// <param name="omitXmlDeclaration">true:省略XML声明;否则为false.默认false，即编写 XML 声明。</param>
        /// <param name="removeDefaultNamespace">是否移除默认名称空间(如果对象定义时指定了:XmlRoot(Namespace = "http://www.xxx.com/xsd")则需要传false值进来)</param>
        /// <returns>序列化后的字符串</returns>
        public static void XmlSerialize<T>(string path, T obj, bool omitXmlDeclaration = false,
                                           bool removeDefaultNamespace = true)
        {
            using (
                XmlWriter xmlwriter = XmlWriter.Create(path,
                                                       new XmlWriterSettings { OmitXmlDeclaration = omitXmlDeclaration }))
            {
                var xmlns = new XmlSerializerNamespaces();
                if (removeDefaultNamespace)
                    xmlns.Add(String.Empty, String.Empty); //在XML序列化时去除默认命名空间xmlns:xsd和xmlns:xsi
                var ser = new XmlSerializer(typeof(T));
                ser.Serialize(xmlwriter, obj, xmlns);
            }
        }

        /// <summary>
        ///     Xml反序列化为对象
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="s">对象序列化后的Xml字符串</param>
        /// <returns></returns>
        public static object XmlDeserialize(Type type, string s)
        {
            using (var sr = new StringReader(s))
            {
                var xz = new XmlSerializer(type);
                return xz.Deserialize(sr);
            }
        }

        /// <summary>
        ///     Json序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t">对象</param>
        /// <returns></returns>
        public static string JsonSerialize<T>(T t)
        {
            var serializer = new JavaScriptSerializer {MaxJsonLength = Int32.MaxValue};
            return serializer.Serialize(t); 
            //return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(t);
            //return JavaScriptConvert.SerializeObject(t);
        }

        /// <summary>
        ///     Json反序列化为对象
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="s">对象序列化后的Xml字符串</param>
        /// <returns></returns>
        public static object JsonDeserialize(Type type, string s)
        {
            return new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(s);
            //return JavaScriptConvert.DeserializeObject(s, type);
        }
    }
}