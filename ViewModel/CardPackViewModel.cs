using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudyApp.Data;
using StudyApp.Services;
using StudyApp.View;
using StudyApp.Models;


namespace StudyApp.ViewModel;
public partial class CardPackViewModel : ObservableObject
{
    public CardPackViewModel()
    {
        LoadCardPacks();
    }

    [ObservableProperty]
    string label;

    [ObservableProperty]
    private List<CardPack> listPacks;

    [ObservableProperty]
    public CardPack currentPack;

    public void LoadCardPacks()
    {
        // update the current card packs
        string filepath = Path.Combine(Globals.DataFolder, Globals.StudySetsFile);
        CurrentPack = CardPackRepository.GetCardPackById(CurrentData.Instance.StudySetID, filepath);
        Label = CurrentPack.Description;

        filepath = Path.Combine(CurrentData.Instance.CardPackPath, Globals.CardPacksFile);
        ListPacks = CardPackRepository.GetCardPacks(filepath);
    }

    [RelayCommand]
    async Task Exam()
    {
        var action = await Shell.Current.DisplayActionSheet("Select Exam Type", "Cancel", null, "Flashcards", "Writing answer");
        Globals.Mode = 1;

        if (action == "Flashcards")
        {
            await Shell.Current.GoToAsync(nameof(PractisePage));
        }
        else if (action == "Writing answer")
        {
            await Shell.Current.GoToAsync(nameof(ExamPage));
        }
    }

    [RelayCommand]
    async Task Add()
    {   // Navigate to the AddCardPage
        await Shell.Current.GoToAsync($"{nameof(AddCardPage)}?Mode=2");
    }

    [RelayCommand]
    async Task Edit()
    {   // Navigate to the EditCardPage
        await Shell.Current.GoToAsync($"{nameof(EditCardPage)}?Mode={3}&IdInt={CurrentData.Instance.StudySetID}");
    }

    [RelayCommand]
    async Task Home()
    {   // Navigate back to the study set selection
        await Shell.Current.GoToAsync("///MainPage");
    }

    [RelayCommand]
    async Task Tap(CardPack SelectedPack)
    {   // Save the selected study set and navigate to the CardPackPage
        if (SelectedPack != null)
        {
            CurrentData.Instance.UpdateSelectedPack(SelectedPack.Id);
            string newDirectory = Path.Combine(CurrentData.Instance.CardPackPath, SelectedPack.Storage);
            CurrentData.Instance.UpdateFlashcardPath(newDirectory);

            var navigationStack = Shell.Current.Navigation.NavigationStack;
            await Shell.Current.GoToAsync(nameof(CardPage));
        }
    }
}