using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace karepay.MailService.diagnostics
{
    public static class WriteEventLog
    {
        private static string LogSource = "Karepay Mail Servisi";
        private static string LogName = "Karepay Mailing Service";

        public static void WriteError(Exception ex,string text)
        {

            if (CheckLogSourceExists())
            {
                EventLog.WriteEntry(LogSource,text + "\n Hata : " + ex.Message + "\n Hata Detayı: " + ex.InnerException + "\n Trace: " + ex.StackTrace, EventLogEntryType.Error);
            }
        }
        public static void WriteInformation(string text)
        {
            if (CheckLogSourceExists())
            {
                EventLog.WriteEntry(LogSource,text, EventLogEntryType.Information);
            }
        }

        public static void WriteWarning(string text)
        {
            if (CheckLogSourceExists())
            {
                EventLog.WriteEntry(LogSource,text, EventLogEntryType.Warning);
            }
        }

        public static bool CheckLogSourceExists()
        {
            try
            {
                if (EventLog.SourceExists(LogSource))
                    return true;
                else
                    EventLog.CreateEventSource(LogSource, LogName);
            }
            catch (Exception ex)
            {
                EventLog.CreateEventSource(LogSource, LogName);
            }

            return true;
        }
    }
}
