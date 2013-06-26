using System;
using System.Diagnostics;
using System.IO;

namespace ATTrafficAnalayzer
{
    public static class Logger
    {
        const string LogFilePath = "App.log";

        public static void Clear()
        {
            File.WriteAllText(LogFilePath, string.Format("LOG FILE STARTING {0}\n\n", DateTime.Today));
        }

        public static void Debug(string message, string module)
        {
            WriteEntry(message, "debug", module);
        }

        public static void Error(string message, string module)
        {
            WriteEntry(message, "error", module);
        }

        public static void Error(Exception ex, string module)
        {
            WriteEntry(ex.Message, "error", module);
        }

        public static void Warning(string message, string module)
        {
            WriteEntry(message, "warning", module);
        }

        public static void Info(string message, string module)
        {
            WriteEntry(message, "info", module);
        }

        private static void WriteEntry(string message, string type, string module)
        {
            Trace.WriteLine(
                    string.Format("{0}\t{1}\t{2}\t{3}",
                                  DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                  type,
                                  module,
                                  message));
        }
    }
}
