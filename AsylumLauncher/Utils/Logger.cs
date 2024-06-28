using System;
using System.IO;

namespace AsylumLauncher.Utils
{
    internal class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppContext.BaseDirectory, "AsylumLauncherLog.txt");

        public static void Log(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - INFO: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log message: {ex.Message}");
            }
        }

        public static void Log(Exception ex)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} - ERROR: {ex.Message}");
                    writer.WriteLine(ex.StackTrace);
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Failed to log exception: {logEx.Message}");
            }
        }
    }
}
