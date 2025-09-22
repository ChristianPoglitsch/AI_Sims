using System;
using System.Collections.Generic;
using System.IO;

namespace AiSims
{
    public enum LoggingInfo
    {
        // Dialogue/Interrupt
        DialogueStart,
        DialogueEnd
    }

    // public static class DebugLogger
    public static class Logger
    {
        static string uniqueId = Guid.NewGuid().ToString();

        private static readonly Dictionary<LoggingInfo, string> logMessages = new Dictionary<LoggingInfo, string>
        {
            // Dialogue/Interrupt
            {LoggingInfo.DialogueStart, "DialogueStart" },
            {LoggingInfo.DialogueEnd, "DialogueEnd" }
        };

        public static string CreateLogEntry(LoggingInfo info, string message)
        {
          // string time = DateTime.Now.ToString("HH:mm:ss.fff");
      
          DateTime utcNow = DateTime.UtcNow;
          long unixTimestampMilliseconds = ((DateTimeOffset)utcNow).ToUnixTimeMilliseconds();

          var entry = $"[{unixTimestampMilliseconds}] [{info}] [{message}]";
            return entry;
        }

        public static void Log(LoggingInfo info, string message, bool includeInFile = false)
        {
            if (includeInFile)
            {
                string fileName;

                {
                  // Check if directory already esists, if not create it
                  if (!Directory.Exists(uniqueId))
                  {
                    Directory.CreateDirectory(uniqueId);
                  }

                  fileName = Path.Combine(uniqueId, $"GameLabLog_{uniqueId}.txt");

                  var entry = CreateLogEntry(info, message);
                  TextWriter tw = new StreamWriter(fileName, true);
                  tw.WriteLine(entry);
                  tw.Close();
                } 
            }
        }
    }
}

