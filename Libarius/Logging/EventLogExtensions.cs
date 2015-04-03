using System.Diagnostics;

namespace Libarius.Logging
{
    public static class EventLogExtensions
    {
        public static void WriteInformation(this EventLog log, string format, params object[] objs)
        {
            log.WriteEntry(string.Format(format, objs), EventLogEntryType.Information);
        }

        public static void WriteError(this EventLog log, string format, params object[] objs)
        {
            log.WriteEntry(string.Format(format, objs), EventLogEntryType.Error);
        }

        public static void WriteWarning(this EventLog log, string format, params object[] objs)
        {
            log.WriteEntry(string.Format(format, objs), EventLogEntryType.Warning);
        }
    }
}