using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Whir.Framework.Extension;

namespace Whir.Framework.Helper
{
    /// <summary>
    ///     辅助类
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        ///     生成树形文件Html
        /// </summary>
        /// <param name="builder">用于存放拼接的Html，由于是递归拼接，调用方法时，传入空的StringBuilder即可</param>
        /// <param name="path">要显示的服务器端文件夹路径（物理路径）</param>
        /// <param name="replacePath">要替换掉的路径部分</param>
        /// <returns></returns>
        public static string GetGuideTree(StringBuilder builder, string path, string replacePath)
        {
            var currentDir = new DirectoryInfo(path);
            DirectoryInfo[] subDirs = currentDir.GetDirectories();
            if (subDirs.Length > 0)
            {
                builder.AppendFormat("<li><span class='folder' path='{0}'>{1}</span>" + Environment.NewLine,
                                     currentDir.FullName.Replace(replacePath, ""), currentDir.Name);
                builder.Append("    <ul>" + Environment.NewLine);
                foreach (DirectoryInfo dir in subDirs)
                {
                    GetGuideTree(builder, dir.FullName, replacePath);
                }

                #region 文件夹下文件

                FileInfo[] files = currentDir.GetFiles();
                if (files.Length > 0)
                {
                    foreach (FileInfo file in files)
                    {
                        string previewUrl = file.FullName.IsImage()
                                                ? GetFileWebUrl(
                                                    file.FullName.Replace(HttpContext.Current.Server.MapPath("~/"), ""))
                                                : string.Empty;
                        builder.AppendFormat(
                            "<li><span class='file' name='{0}' img='{1}' path='{2}'>{0}</span>" + Environment.NewLine,
                            file.Name,
                            previewUrl, file.FullName.Replace(replacePath, ""));
                    }
                }

                builder.Append("    </ul>" + Environment.NewLine);

                #endregion

                builder.Append("</li>" + Environment.NewLine);
            }
            else
            {
                builder.AppendFormat(
                    "<li  class='closed'><span class='folder' path='{0}'>{1}</span>" + Environment.NewLine,
                    currentDir.FullName.Replace(replacePath, ""), currentDir.Name);

                #region 文件夹下文件

                FileInfo[] files = currentDir.GetFiles();
                if (files.Length > 0)
                {
                    builder.Append("    <ul>" + Environment.NewLine);
                    foreach (FileInfo file in files)
                    {
                        string previewUrl = file.FullName.IsImage()
                                                ? GetFileWebUrl(
                                                    file.FullName.Replace(HttpContext.Current.Server.MapPath("~/"), ""))
                                                : string.Empty;
                        builder.AppendFormat(
                            "<li><span class='file' name='{0}' img='{1}' path='{2}'>{0}</span>" + Environment.NewLine,
                            file.Name,
                            previewUrl, file.FullName.Replace(replacePath, ""));
                    }
                    builder.Append("    </ul>" + Environment.NewLine);
                }

                #endregion
            }
            return builder.ToString();
        }

        public static string GetFileWebUrl(string filePath)
        {
            if (filePath.IsEmpty())
            {
                return string.Empty;
            }
            filePath = filePath.Replace("\\", "/");
            if (filePath.StartsWith("/"))
            {
                filePath = filePath.TrimStart('/');
            }
            return VirtualPathUtility.AppendTrailingSlash(HttpContext.Current.Request.ApplicationPath) + filePath;
        }

        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 逐行读取文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<string> ReadFileForLines(string filePath)
        {
            var lines = new List<string>();
            using (var sr = new StreamReader(filePath, Encoding.UTF8))
            {
                String input;
                while ((input = sr.ReadLine()) != null)
                {
                    lines.Add(input);
                }
            }
            return lines;
        }

        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        public static void WriteFile(string filePath, string content)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (var stream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    Encoding encode = Encoding.UTF8;
                    //获得字节数组
                    byte[] data = encode.GetBytes(content);
                    //开始写入
                    stream.Write(data, 0, data.Length);
                    //清空缓冲区、关闭流
                    stream.Flush();
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }
        public static void GetFiles(string dir, List<string> list)
        {
            //添加文件
            list.AddRange(Directory.GetFiles(dir));

            //如果是目录，则递归
            DirectoryInfo[] directories = new DirectoryInfo(dir).GetDirectories();
            foreach (DirectoryInfo item in directories)
            {
                GetFiles(item.FullName, list);
            }
        }
    }
}