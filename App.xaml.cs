using StudyApp.Models;
using StudyApp.Data;
using StudyApp.View;
using StudyApp.Services;

namespace StudyApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new LoadingPage();
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            if (!Directory.Exists(Globals.DataFolder))
            {
                await CardPackRepository.CreateStarterPackAsync();
            }

            await LoadSettingsAsync();
            MainPage = new AppShell();
        }

        private async Task LoadSettingsAsync()
        {
            var settingsService = new UserSettingsService();
            UserSettings loaded = await settingsService.LoadAsync();
            UserSettingsCopy.ApiKey = loaded.ApiKey;
            UserSettingsCopy.NativeLanguage = loaded.NativeLanguage;
            UserSettingsCopy.ShowApiKeyPopup = loaded.ShowApiKeyPopup;
        }

    }
}
