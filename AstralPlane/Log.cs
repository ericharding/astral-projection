using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Astral.Plane
{
    internal static class Log
    {
        static StreamWriter GetWriter()
        {
            StreamWriter writer = new StreamWriter("map_access_log.txt");
            writer.AutoFlush = true;
            return writer;
        }

        static StringBuilder _stringLog = new StringBuilder();

        static Log()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using (var writer = GetWriter())
            {
                writer.WriteLine(_stringLog.ToString());
                writer.Flush();
            }
        }

        public static void log(string format, params object[] args)
        {
            string logmsg = string.Format(format, args);
            int thread = Thread.CurrentThread.ManagedThreadId;
            DateTime time = DateTime.Now;

            string logLine = string.Format("{0:0000} {1} {2}", thread, time.ToString("hh:mm:ss.ffff"), logmsg);

            _stringLog.AppendLine(logLine);
        }

    }
}
