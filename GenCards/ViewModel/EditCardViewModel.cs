using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
#if IOS
using UIKit;
#endif

using StudyApp.Data;
using StudyApp.Services;
using StudyApp.View;
using StudyApp.Models;

namespace StudyApp.ViewModel;
[QueryProperty(nameof(CardIdGuid), "IdGuid")]
[QueryProperty(nameof(CardIdInt), "IdInt")]
[QueryProperty(nameof(EditMode), "Mode")]
public partial class EditCardViewModel : ObservableObject
{
    private Flashcard? flashcard;
    private CardPack? cardPack;
    private string? filepath;
    private bool is_ID_init = false;
    private List<string> ImagesToDelete = new List<string>();
    string? secondLanguage;

    [ObservableProperty]
    string title = "";
    [ObservableProperty]
    string text1 = "";
    [ObservableProperty]
    string text2 = "";
    [ObservableProperty]
    bool isButtonVisible = false;

    // user entries
    [ObservableProperty]
    string entry1 = "";
    [ObservableProperty]
    string entry2 = "";
    [ObservableProperty]
    bool isImg1Added = false;
    [ObservableProperty]
    string addImg1Text = "🖼️";
    [ObservableProperty]
    string? entryImg1Name;
    [ObservableProperty]
    bool isImg2Added = false;
    [ObservableProperty]
    string addImg2Text = "🖼️";
    [ObservableProperty]
    string? entryImg2Name;

    [ObservableProperty]
    bool shareVisible = false;
    [ObservableProperty]
    bool languageVisible = false;
    [ObservableProperty]
    string shareTitle = "";

    private int _editMode;
    public int EditMode
    {
        get => _editMode;
        set
        {
            _editMode = value;
            InitVariables();
        }
    }

    private Guid _cardIdGuid;
    public string CardIdGuid
    {
        get => _cardIdGuid.ToString();
        set
        {
            _cardIdGuid = Guid.Parse(value);
            is_ID_init = true;
            InitVariables();
        }
    }

    private int _cardIdInt;
    public string CardIdInt
    {
        get => _cardIdInt.ToString();
        set
        {
            _cardIdInt = int.Parse(value);
            is_ID_init = true;
            InitVariables();
        }
    }

    public void InitVariables()
    {
        if (EditMode == 0 || !is_ID_init)
            return;     // wait for initialization

        else if ( EditMode == 1)
        {
            Title = "Edit flashcard";
            Text1 = "Question";
            Text2 = "Answer";
            IsButtonVisible = true;
            flashcard = FlashcardRepository.GetFlashcardById(_cardIdGuid);
            if (flashcard != null)
            {
                Entry1 = flashcard.Question;
                Entry2 = flashcard.Answer;
                EntryImg1Name = flashcard.Img1Name;
                EntryImg2Name = flashcard.Img2Name;

                if (EntryImg1Name != null)
                {
                    IsImg1Added = true;
                    AddImg1Text = "🗑️";
                }
                if (EntryImg2Name != null)
                {
                    IsImg2Added = true;
                    AddImg2Text = "🗑️";
                }
            }
        }

        else if (EditMode == 2)
        {
            Title = "Edit pack of cards";
            Text1 = "Name";
            Text2 = "Description";
            ShareVisible = true;
            ShareTitle = "SHARE THIS PACK OF FLASHCARDS";
            filepath = Path.Combine(CurrentData.Instance.CardPackPath, Globals.CardPacksFile);
            cardPack = CardPackRepository.GetCardPackById(_cardIdInt, filepath);
            if (cardPack != null)
            {
                Entry1 = cardPack.Name;
                Entry2 = cardPack.Description;
            }
        }

        else if (EditMode == 3)
        {
            Title = "Edit study set";
            Text1 = "Name";
            Text2 = "Description";
            LanguageVisible = true;
            ShareVisible = true;
            ShareTitle = "UPLOAD EXISTING FLASHCARDS";
            filepath = Path.Combine(Globals.DataFolder, Globals.StudySetsFile);
            cardPack = CardPackRepository.GetCardPackById(_cardIdInt, filepath);
            if (cardPack != null)
            {
                Entry1 = cardPack.Name;
                Entry2 = cardPack.Description;
            }
        }
    }

    public void UpdateCard()
    {
        if (EditMode == 1)
        {
            flashcard.Question = Entry1;
            flashcard.Answer = Entry2;
            flashcard.Img1Name = EntryImg1Name;
            flashcard.Img2Name = EntryImg2Name;
            FlashcardRepository.UpdateFlashcard(_cardIdGuid, flashcard);
        }
        else if (EditMode == 2 || EditMode == 3)
        {
            cardPack.Name = Entry1;
            cardPack.Description = Entry2;
            cardPack.Language = secondLanguage;
            CardPackRepository.UpdateCardPack(_cardIdInt, cardPack, filepath);
        }
    }

    private bool IsIOS14OrLater()
    {
        #if IOS
                return UIDevice.CurrentDevice.CheckSystemVersion(14, 0);
        #else
            return true; // Default to true for other platforms
        #endif
    }

    private async Task ShowCardPackExportOptionsAsync()
    {
        try
        {
            if (DeviceInfo.Platform == DevicePlatform.iOS && !IsIOS14OrLater())
            {
                await CardPackRepository.ShareCardPackAsync();
            }
            else
            {
                string action = await Shell.Current.DisplayActionSheet(
                    "Export flashcards as a .json file",
                    "Cancel",
                    null,
                    "Save File",
                    "Share File"
                );

                if (action == "Save File")
                {
                    await CardPackRepository.ExportCardPackAsync();
                }
                else if (action == "Share File")
                {
                    await CardPackRepository.ShareCardPackAsync();
                }
            }
        }
        catch (Exception ex)
        {
            await Toast.Make($"Error: {ex.Message}").Show();
        }
    }

    async Task<string?> LoadImgAsync()
    { // TODO presunout zvlast
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
                // save image to local path
                var localFilePath = Path.Combine(CurrentData.Instance.CardPackPath, result.FileName);

                var directory = CurrentData.Instance.CardPackPath;
                var originalFileName = Path.GetFileNameWithoutExtension(result.FileName);
                var extension = Path.GetExtension(result.FileName);
                int counter = 1;
                while (File.Exists(localFilePath))
                {
                    var newFileName = $"{originalFileName}_{counter}{extension}";
                    localFilePath = Path.Combine(directory, newFileName);
                    counter++;
                }

                using (var sourceStream = await result.OpenReadAsync())
                using (var destinationStream = File.OpenWrite(localFilePath))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }

                //Errortext = "Image successfully saved to local storage.";
                return localFilePath;
            }
        }
        catch (Exception ex)
        {
            //Errortext = $"Error saving image: {ex.Message}";
        }

        return null;
    }

    [RelayCommand]
    async Task AddImg1()
    {
        if (IsImg1Added == false)
        {
            // Add image
            EntryImg1Name = await LoadImgAsync();
            if (EntryImg1Name != null)
            {
                IsImg1Added = true;
                AddImg1Text = "🗑️";
            }
        }
        else
        {
            // Delete image
            if (EntryImg1Name != null)
            {
                ImagesToDelete.Add(EntryImg1Name);
            }
            
            EntryImg1Name = null;
            IsImg1Added = false;
            AddImg1Text = "🖼️";
        }
    }

    [RelayCommand]
    async Task AddImg2()
    {
        if (IsImg2Added == false)
        {
            // Add image
            EntryImg2Name = await LoadImgAsync();
            if (EntryImg2Name != null)
            {
                IsImg2Added = true;
                AddImg2Text = "🗑️";
            }
        }
        else
        {
            // Delete image
            if (EntryImg2Name != null)
            {
                ImagesToDelete.Add(EntryImg2Name);
            }
            EntryImg2Name = null;
            IsImg2Added = false;
            AddImg2Text = "🖼️";
        }
    }

    [RelayCommand]
    async Task EditLanguage()
    {
        secondLanguage = await Shell.Current.DisplayPromptAsync(
            "Language of the Study Set",
            "Enter the second language you want to use in this set. " +
            "It will be used for automatic translation in bilingual flashcards.",
            initialValue: cardPack.Language);
    }

    [RelayCommand]
    async Task ShareFlashcards()
    {
        if (EditMode == 3)
            // add shared pack of flashcards
            await Shell.Current.GoToAsync($"{nameof(AddCardPage)}?Mode=4");
        if (EditMode == 2)
            // share pack of flashcards
            await ShowCardPackExportOptionsAsync();
    }

    [RelayCommand]
    async Task Save()
    {   // when saving changes, user updates to entries are saved to the selected flashcard
        UpdateCard();
        for (int i = 0; i < ImagesToDelete.Count; i++)
        {
            FlashcardRepository.DeteleImage(ImagesToDelete[i]);
        }

        await GoBack();
    }

    [RelayCommand]
    async Task Delete()
    {   // delete the selected flashcard and navigate back to the CardPackPage
        string cardType = Title.Length >= 5 ? Title.Substring(5) : string.Empty;
        bool confirmed = await Shell.Current.DisplayAlert(
                        "Confirm Deletion",
                        $"Do you really want to delete this {cardType}?",
                        "Yes", "No");

        if (confirmed)
        {
            Globals.Mode = 0;
            if (EditMode == 1)
            {
                FlashcardRepository.DeleteFlashcard(_cardIdGuid);
                await GoBack();
            }
            else if (EditMode == 2 || EditMode == 3)
            {
                CardPackRepository.DeleteCardPack(_cardIdInt, filepath);
                await Shell.Current.GoToAsync("../..");
            }
        }
    }

    private void DeleteSharedZipFiles()
    {
        string cacheDir = FileSystem.CacheDirectory;
        var zipFiles = Directory.GetFiles(cacheDir, "sharedFlashcards_*.zip");

        foreach (var file in zipFiles)
        {
            try { File.Delete(file); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Could not delete file: {ex.Message}");
            }
        }
    }


    [RelayCommand]
    async Task GoBack()
    {   
        // delete temp zip after sharing
        DeleteSharedZipFiles();
        // Navigate back to the CardPackPages
        await Shell.Current.GoToAsync("..");
    }
}

