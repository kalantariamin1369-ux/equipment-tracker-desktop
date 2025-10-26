using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EquipmentTracker
{
    public static class Utilities
    {
        public static void LogError(string message, Exception ex = null)
        {
            try
            {
                var logPath = Path.Combine(Application.StartupPath, "Logs");
                Directory.CreateDirectory(logPath);
                
                var logFile = Path.Combine(logPath, $"error_log_{DateTime.Now:yyyyMMdd}.txt");
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                
                if (ex != null)
                {
                    logEntry += $"\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
                }
                
                File.AppendAllText(logFile, logEntry + "\n\n");
            }
            catch
            {
                // Silent fail for logging errors
            }
        }

        public static void ExportToCsv<T>(List<T> data, string filePath, Dictionary<string, Func<T, object>> columnMappings)
        {
            var csv = new StringBuilder();
            
            // Add headers
            csv.AppendLine(string.Join(",", columnMappings.Keys));
            
            // Add data rows
            foreach (var item in data)
            {
                var values = new List<string>();
                foreach (var mapping in columnMappings.Values)
                {
                    var value = mapping(item)?.ToString() ?? "";
                    // Escape commas and quotes
                    if (value.Contains(",") || value.Contains("\""))
                    {
                        value = $"\"{value.Replace("\"", "\"\"")}\"";
                    }
                    values.Add(value);
                }
                csv.AppendLine(string.Join(",", values));
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }

        public static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }
    }

    public static class Constants
    {
        public const string APP_NAME = "Equipment Tracker";
        public const string APP_VERSION = "1.0.0";
        public const string DATABASE_FILE = "equipment.db";
    }
}