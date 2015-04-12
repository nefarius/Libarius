using System.Diagnostics;

namespace Libarius.Logging
{
    /// <summary>
    ///     Provides extension methods for the EventLog class.
    /// </summary>
    public static class EventLogExtensions
    {
        /// <summary>
        ///     Writes an information to the event log.
        /// </summary>
        /// <param name="log">The event log to extend.</param>
        /// <param name="format">The format string of the message.</param>
        /// <param name="objs">Additional optional parameters of the message.</param>
        public static void WriteInformation(this EventLog log, string format, params object[] objs)
        {
            log.WriteEntry(string.Format(format, objs), EventLogEntryType.Information);
        }

        /// <summary>
        ///     Writes an error to the event log.
        /// </summary>
        /// <param name="log">The event log to extend.</param>
        /// <param name="format">The format string of the message.</param>
        /// <param name="objs">Additional optional parameters of the message.</param>
        public static void WriteError(this EventLog log, string format, params object[] objs)
        {
            log.WriteEntry(string.Format(format, objs), EventLogEntryType.Error);
        }

        /// <summary>
        ///     Writes a warning to the event log.
        /// </summary>
        /// <param name="log">The event log to extend.</param>
        /// <param name="format">The format string of the message.</param>
        /// <param name="objs">Additional optional parameters of the message.</param>
        public static void WriteWarning(this EventLog log, string format, params object[] objs)
        {
            log.WriteEntry(string.Format(format, objs), EventLogEntryType.Warning);
        }
    }
}