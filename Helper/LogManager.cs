using System;
using System.IO;
using System.Web;

namespace Whir.Framework.Helper
{
    public class LogManager
    {
        protected static string LogPath = HttpContext.Current.Server.MapPath("~/Log");
        public static void Log(string msg)
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            var filePath = Path.Combine(LogPath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            using (StreamWriter w = File.AppendText(filePath))
            {
                w.WriteLine("# " + DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss ") + msg);
                w.Close();
            }
        }
    }
}
