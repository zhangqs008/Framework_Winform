using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace Whir.Framework.Helper
{
    /// <summary>
    /// zip文件压缩
    /// </summary>
    public class ZipUtil
    {
        /// <summary>
        /// 压缩文件 调用 ZipUtil.CreateZip(@"D:\Temp\forzip", @"D:\Temp2\forzip.zip") 即可。
        /// </summary>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationZipFilePath"></param>
        public static bool CreateZip(string sourceFilePath, string destinationZipFilePath)
        {
            try
            {
                if (sourceFilePath[sourceFilePath.Length - 1] != Path.DirectorySeparatorChar)
                {
                    sourceFilePath += Path.DirectorySeparatorChar;
                }
                var zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));
                zipStream.SetLevel(6); // 压缩级别 0-9
                CreateZipFiles(sourceFilePath, zipStream, sourceFilePath);
                zipStream.Finish();
                zipStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Log("文件压缩异常：" + ex.Message+ex.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 递归压缩文件
        /// </summary>
        /// <param name="sourceFilePath">待压缩的文件或文件夹路径</param>
        /// <param name="zipStream">打包结果的zip文件路径（类似 D:\WorkSpace\a.zip）,全路径包括文件名和.zip扩展名</param>
        /// <param name="staticFile"></param>
        private static void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream, string staticFile)
        {
            var crc = new Crc32();
            string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
            foreach (string file in filesArray)
            {
                if (Directory.Exists(file)) //如果当前是文件夹，递归
                {
                    CreateZipFiles(file, zipStream, staticFile);
                }

                else //如果是文件，开始压缩
                {
                    FileStream fileStream = File.OpenRead(file);

                    var buffer = new byte[fileStream.Length];
                    fileStream.Read(buffer, 0, buffer.Length);
                    string tempFile = file.Substring(staticFile.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                    var entry = new ZipEntry(tempFile) { DateTime = DateTime.Now, Size = fileStream.Length };

                    fileStream.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    zipStream.PutNextEntry(entry);

                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}