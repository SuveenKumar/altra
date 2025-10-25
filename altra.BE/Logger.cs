using System;
using System.IO;
namespace altra.BE
{
    public static class Logger
    {
        private static string logFilePath = "log/intraday_log.txt"; // Change the file path as needed

        public static void Log(string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write log: {ex.Message}");
            }
        }
    }
}
