using GarrysmodDesktopAddonExtractor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarrysmodDesktopAddonExtractor.Services
{
    public class SettingsService
    {
        public static async Task<SettingsInfo> GetSettingsAsync()
        {
            var settingsInfo = new SettingsInfo();
            string filePath = GetSettingsFilePath();
            string directoryPath = GetSettingsDirectoryPath();

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            if (File.Exists(filePath))
            {
                try
                {
                    string fileCOntent = await File.ReadAllTextAsync(filePath);
                    settingsInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsInfo>(fileCOntent);
                }
                catch { }
            }

            return settingsInfo;
        }

        public static async Task WriteSettingsAsync(SettingsInfo settingsInfo)
        {
            string fileCOntent = Newtonsoft.Json.JsonConvert.SerializeObject(settingsInfo);
            string directoryPath = GetSettingsDirectoryPath();
            string filePath = GetSettingsFilePath();

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            await File.WriteAllTextAsync(filePath, fileCOntent);
        }

        private static string GetSettingsFilePath() => Path.Combine(GetSettingsDirectoryPath(), "settings.json");

        private static string GetSettingsDirectoryPath() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "application");
    }
}
