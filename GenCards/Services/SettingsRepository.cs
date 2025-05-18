using Newtonsoft.Json;
using StudyApp.Models;

namespace StudyApp.Services
{
    public class UserSettingsService
    {
        private static string SettingsFilePath => Path.Combine(FileSystem.AppDataDirectory, "UserSettings.json");

        public async Task<UserSettings> LoadAsync()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                    return new UserSettings();

                var json = await File.ReadAllTextAsync(SettingsFilePath);

                if (string.IsNullOrWhiteSpace(json))
                    return new UserSettings();

                return JsonConvert.DeserializeObject<UserSettings>(json) ?? new UserSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                return new UserSettings();
            }
        }

        public async Task SaveAsync(UserSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                await File.WriteAllTextAsync(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}
