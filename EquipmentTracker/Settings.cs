using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace EquipmentTracker
{
    public class AppSettings
    {
        public string Theme { get; set; } = "Light";
        public int DefaultTab { get; set; } = 0;
        public bool ShowLowStockAlerts { get; set; } = true;
        public string LastImportPath { get; set; } = "";
        public string LastExportPath { get; set; } = "";
        
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "EquipmentTracker",
            "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Utilities.LogError("Error loading settings", ex);
            }
            
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Utilities.LogError("Error saving settings", ex);
            }
        }

        public void ApplyTheme(Form form)
        {
            if (Theme == "Dark")
            {
                ApplyDarkTheme(form);
            }
            else
            {
                ApplyLightTheme(form);
            }
        }

        private void ApplyDarkTheme(Form form)
        {
            form.BackColor = Color.FromArgb(45, 45, 48);
            form.ForeColor = Color.White;
        }

        private void ApplyLightTheme(Form form)
        {
            form.BackColor = SystemColors.Control;
            form.ForeColor = SystemColors.ControlText;
        }
    }
}