using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudyApp.Data;
using StudyApp.Services;
using StudyApp.View;
using StudyApp.Models;


namespace StudyApp.ViewModel;
public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private List<CardPack> listPacks;

    public HomeViewModel()
    {
        LoadStudySets();
    }

    public void LoadStudySets()
    {   // update the current card packs
        string filepath = Path.Combine(Globals.DataFolder, Globals.StudySetsFile);
        ListPacks = CardPackRepository.GetCardPacks(filepath);
    }

    public async Task CheckApiAsync()
    {
        if (string.IsNullOrWhiteSpace(UserSettingsCopy.ApiKey) && UserSettingsCopy.ShowApiKeyPopup)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                var popup = new ApiKeyPopup();
                Application.Current.MainPage.ShowPopup(popup);
            });
        }
    }


    [RelayCommand]
    async Task Settings()
    {   // Navigate to the AddCardPage
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    async Task Add()
    {   // Navigate to the AddCardPage
        await Shell.Current.GoToAsync($"{nameof(AddCardPage)}?Mode=3");
    }

    [RelayCommand]
    async Task Tap(CardPack SelectedStudySet)
    {   // Save the selected study set and navigate to the CardPackPage
        if (SelectedStudySet != null)
        {
            CurrentData.Instance.UpdateSelectedStudySet(SelectedStudySet.Id);
            string newDirectory = Path.Combine(Globals.DataFolder, SelectedStudySet.Storage);
            CurrentData.Instance.UpdateCardPackPath(newDirectory);

            var navigationStack = Shell.Current.Navigation.NavigationStack;
            await Shell.Current.GoToAsync(nameof(CardPackPage));
        }
    }

}
