using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AiSims
{
    public enum LoggingInfo
    {
        // Dialogue/Interrupt
        DialogueUser,
        DialoagNpc,
        Scene
    }

    // public static class DebugLogger
    public static class Logger
    {
        static string filePath = Application.persistentDataPath;
        static string uniqueId = Guid.NewGuid().ToString();

        static string fullPath = filePath; //  Path.Combine(filePath, uniqueId);

        private static readonly Dictionary<LoggingInfo, string> logMessages = new Dictionary<LoggingInfo, string>
        {
            // Dialogue/Interrupt
            {LoggingInfo.DialogueUser, "DialogueUser" },
            {LoggingInfo.DialoagNpc, "DialoagNpc" },
            {LoggingInfo.Scene, "Scene" }
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
                Debug.Log("Filepath: " + fullPath);
                // Check if directory already esists, if not create it
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }

                string fileName = Path.Combine(fullPath, $"GameLabLog_{uniqueId}.txt");

                var entry = CreateLogEntry(info, message);
                TextWriter tw = new StreamWriter(fileName, true);
                tw.WriteLine(entry);
                tw.Close();
            }
        }
    }
}
