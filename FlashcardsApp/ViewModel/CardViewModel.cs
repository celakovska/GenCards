using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudyApp.Data;
using StudyApp.Services;
using StudyApp.View;
using StudyApp.Models;


namespace StudyApp.ViewModel;
public partial class CardViewModel : ObservableObject
{
    [ObservableProperty]
    string label = "";

    [ObservableProperty]
    private List<Flashcard> listCards;

    [ObservableProperty]
    public CardPack currentPack;

    [ObservableProperty]
    private bool hasFlashcards = false;

    public void LoadFlashcards()
    {   
        string filepath = Path.Combine(CurrentData.Instance.CardPackPath, Globals.CardPacksFile);
        CurrentPack = CardPackRepository.GetCardPackById(CurrentData.Instance.PackID, filepath);
        Label = CurrentPack.Description;

        // Update the current flashcards
        ListCards = FlashcardRepository.GetFlashcards();
        if (ListCards.Count > 0)
        {
            HasFlashcards = true;
        };
    }

    [RelayCommand]
    async Task Exam()
    {
        var action = await Shell.Current.DisplayActionSheet("Select Exam Type", "Cancel", null, "Flashcards", "Writing answer");
        Globals.Mode = 0;

        if (action == "Flashcards")
        {
            await Shell.Current.GoToAsync(nameof(PractisePage));
        }
        else if (action == "Writing answer")
        {
            await Shell.Current.GoToAsync(nameof(ExamPage));
        }
    }

    async Task<string?> LoadImgAsync()
    {
        try
        {
            var action = await Shell.Current.DisplayActionSheet("Select Image", "Cancel", null, "Gallery", "Camera");

            FileResult? result = null;

            if (action == "Gallery")
            {
                result = await MediaPicker.PickPhotoAsync();
            }
            else if (action == "Camera")
            {
                result = await MediaPicker.CapturePhotoAsync();
            }

            if (result != null)
            {
                var localFilePath = Path.Combine(CurrentData.Instance.CardPackPath, result.FileName);

                using (var sourceStream = await result.OpenReadAsync())
                using (var destinationStream = File.OpenWrite(localFilePath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                //errortext = "Image successfully saved to local storage.";
                return localFilePath;
            }
        }
        catch (Exception ex)
        {
            //errortext = $"Error saving image: {ex.Message}";
        }

        return null;
    }


    [RelayCommand]
    async Task Add()
    {   // Navigate to the AddCardPage
        var action = await Shell.Current.DisplayActionSheet("Create new flashcard", "Cancel", null, "Start with empty flashcard", "Translate text from photo");

        if (action == "Start with empty flashcard")
        {
            await Shell.Current.GoToAsync($"{nameof(AddCardPage)}?Mode=1");
        }
        else if (action == "Translate text from photo")
        {
            string? imagepath = await LoadImgAsync();
            if (imagepath != null)
                await Shell.Current.GoToAsync($"{nameof(DrawPage)}?imagepath={imagepath}");
        }
    }

    [RelayCommand]
    async Task Edit()
    {   // Navigate to the EditCardPage
        await Shell.Current.GoToAsync($"{nameof(EditCardPage)}?Mode={2}&IdInt={CurrentData.Instance.PackID}");
    }

    [RelayCommand]
    async Task Home()
    {   // Navigate back to the study set selection
        await Shell.Current.GoToAsync("///MainPage");
    }

    [RelayCommand]
    async Task Tap(Flashcard SelectedCard)
    {   // Navigate to the EditCardPage
        if (SelectedCard != null)
        {
            await Shell.Current.GoToAsync($"{nameof(EditCardPage)}?IdGuid={((Flashcard)SelectedCard).Id}&Mode=1");

        }
    }
}