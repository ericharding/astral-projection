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
        static Lazy<StreamWriter> _writer = new Lazy<StreamWriter>(() =>
        {
            Stream s = File.OpenWrite("map_access_log.txt");
                StreamWriter writer = new StreamWriter(s);
            writer.AutoFlush = true;
            return writer;
        });

        static Log()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (_writer.IsValueCreated)
            {
                _writer.Value.Flush();
                _writer.Value.Close();
            }
        }

        public static void log(string format, params object[] args)
        {
            string logmsg = string.Format(format, args);
            int thread = Thread.CurrentThread.ManagedThreadId;
            DateTime time = DateTime.Now;

            string logLine = string.Format("{0:0000} {1} {2}", thread, time.ToString("hh:mm:ss.ffff"), logmsg);

            _writer.Value.WriteLine(logLine);
        }

    }
}
