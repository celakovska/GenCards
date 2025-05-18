using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudyApp.Services;
using StudyApp.Data;
using StudyApp.Models;

namespace StudyApp.ViewModel
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ChatGptService _chatGptService;

        [ObservableProperty]
        private string? apiKeyCopy = UserSettingsCopy.ApiKey;

        [ObservableProperty]
        private string apiKeyTitle = "";

        [ObservableProperty]
        private string defLanguage = UserSettingsCopy.NativeLanguage;

        public SettingsViewModel(ChatGptService chatGptService)
        {
            _chatGptService = chatGptService;

            if (string.IsNullOrWhiteSpace(UserSettingsCopy.ApiKey))
            {
                ApiKeyTitle = "Enter OpenAI API key: ";
            }
            else
            {
                ApiKeyTitle = "Your OpenAI API key: ";
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            UserSettingsCopy.ApiKey = ApiKeyCopy;
            _chatGptService.UpdateApiKey(ApiKeyCopy);
            ApiKeyTitle = "Your OpenAI API key: ";

            UserSettingsCopy.NativeLanguage = DefLanguage;
            UserSettings input = new UserSettings { ApiKey = ApiKeyCopy, ShowApiKeyPopup = false, NativeLanguage = DefLanguage };
            var settingsService = new UserSettingsService();
            await settingsService.SaveAsync(input);

            await Exit();
        }

        [RelayCommand]
        private async Task Exit()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
