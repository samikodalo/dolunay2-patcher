using System;
using System.IO;
using System.Text;

namespace Dolunay2Launcher
{
    public static class Log
    {
        static readonly object Gate = new object();
        static string _file = "";

        static string File()
        {
            if (!string.IsNullOrEmpty(_file)) return _file;
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(dir);
                _file = Path.Combine(dir, "launcher_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            }
            catch (Exception) { _file = ""; }
            return _file;
        }

        public static void Info(string msg)
        {
            try
            {
                string f = File();
                if (string.IsNullOrEmpty(f)) return;
                lock (Gate)
                    System.IO.File.AppendAllText(f,
                        "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + msg + "\r\n",
                        Encoding.UTF8);
            }
            catch (Exception) { }
        }

        public static string Latest()
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(dir)) return "";
                string best = "";
                DateTime bt = DateTime.MinValue;
                foreach (var f in Directory.GetFiles(dir, "launcher_*.txt"))
                {
                    var w = System.IO.File.GetLastWriteTime(f);
                    if (w > bt) { bt = w; best = f; }
                }
                return best;
            }
            catch (Exception) { return ""; }
        }
    }
}
