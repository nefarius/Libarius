using System;
using System.Diagnostics;
using System.IO;

namespace Libarius.Logging
{
    public class SimpleLog
    {
        private FileStream logFile;
        private TextWriterTraceListener logFileListener;
        private TextWriterTraceListener consoleListener;

        public void Initialize(string fileName)
        {
            logFile = new FileStream(fileName, FileMode.OpenOrCreate);
            logFileListener = new TextWriterTraceListener(logFile);
            consoleListener = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Add(logFileListener);
            Trace.Listeners.Add(consoleListener);
        }

        private SimpleLog()
        {
            Trace.Listeners.Clear();
        }

        public static readonly SimpleLog Instance = new SimpleLog();

        public void WriteLine(string line)
        {
            DateTime now = DateTime.Now;
            string date = now.ToShortDateString();
            string time = now.ToLongTimeString();

            Trace.WriteLine(string.Format("{0}, {1} - {2}", date, time, line));
            Trace.Flush();
        }

        public void WriteLine(string format, params object[] list)
        {
            WriteLine(string.Format(format, list));
        }

        public void WriteLine(object obj)
        {
            WriteLine(obj.ToString());
        }
    }
}

