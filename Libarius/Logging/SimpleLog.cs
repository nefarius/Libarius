using System;
using System.Diagnostics;
using System.IO;

namespace Libarius.Logging
{
    public class SimpleLog
    {
        private FileStream logFile;
        private TextWriterTraceListener _logFileListener;
        private TextWriterTraceListener _consoleListener;

        public void Initialize(string fileName)
        {
            logFile = new FileStream(fileName, FileMode.OpenOrCreate);
            _logFileListener = new TextWriterTraceListener(logFile);
            _consoleListener = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Add(_logFileListener);
            Trace.Listeners.Add(_consoleListener);
        }

        private SimpleLog()
        {
            Trace.Listeners.Clear();
        }

        public static readonly SimpleLog Instance = new SimpleLog();

        public void WriteLine(string line)
        {
            var now = DateTime.Now;
            var date = now.ToShortDateString();
            var time = now.ToLongTimeString();

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

