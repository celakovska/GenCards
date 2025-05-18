using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudyApp.Data;
using StudyApp.Services;
using StudyApp.Models;

namespace StudyApp.ViewModel;

[QueryProperty(nameof(EditMode), "Mode")]
public partial class AddCardViewModel : ObservableObject
{
    private readonly ChatGptService _chatGptService;

    public AddCardViewModel(ChatGptService chatGptService)
    {
        _chatGptService = chatGptService;
    }

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

    private Flashcard? flashcard;

    [ObservableProperty]
    string title;
    [ObservableProperty]
    string translateTitle = $"Translate to {UserSettingsCopy.NativeLanguage}";
    [ObservableProperty]
    string text1 = "";
    [ObservableProperty]
    string text2 = "";
    [ObservableProperty]
    bool isButtonVisible = false;
    [ObservableProperty]
    bool languageVisible = false;
    [ObservableProperty]
    string saveTitle = "SAVE";

    // User entries
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

    string? secondLanguage;
    [ObservableProperty]
    private bool isLoadingVisible = false;
    [ObservableProperty]
    private string loadingTitle = "";

    private void InitVariables()
    {
        if (EditMode == 1)
        {
            Title = "Add new flashcard";
            Text1 = "Question";
            Text2 = "Answer";
            IsButtonVisible = true;

            string filepath = Path.Combine(Globals.DataFolder, Globals.StudySetsFile);
            CardPack currentPack = CardPackRepository.GetCardPackById(CurrentData.Instance.StudySetID, filepath);
            secondLanguage = currentPack.Language;

            if (secondLanguage != null)
                TranslateTitle += $"\\{secondLanguage}";
        }
        else
        {
            Title = "Add new pack for flashcards";
            Text1 = "Name";
            Text2 = "Description";

            if (EditMode == 3)
                LanguageVisible = true;
            if (EditMode == 4)
                SaveTitle = "Upload the file";
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
            FlashcardRepository.DeteleImage(EntryImg1Name);
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
            FlashcardRepository.DeteleImage(EntryImg2Name);
            EntryImg2Name = null;
            IsImg2Added = false;
            AddImg2Text = "🖼️";
        }
    }


    [RelayCommand]
    async Task Generate()
    {
        LoadingTitle = "Generating...";
        IsLoadingVisible = true;
        if (EditMode == 1)
        {
            if (IsImg1Added == true)
            {
                string? PayloadImg1 = _chatGptService.ConvertImageToBase64(EntryImg1Name);
                string requestType =
                    "You are a helpful assistant that helps the user create flashcards. " +
                    "Each flashcard has a title and a picture, both of which appear on the front. " +
                    "Your task is to generate a clear, relevant, and concise answer that appears on the back of the flashcard. " +
                    "Use the picture if it provides meaningful information, but ignore it if it is only illustrative. " +
                    "Generate the answer in the same language as the question, unless the user is explicitly asking for a translation. " +
                    "Keep the answer short and informative so it fits well within the flashcard layout.";
                string model = "gpt-4o-mini";
                Entry2 = await _chatGptService.GenerateAiAnswerAsync(requestType, Entry1, model, PayloadImg1);
            }

            else
            {
                string requestType =
                    "You are a helpful assistant that helps the user create flashcards. " +
                    "The user has written a question that will appear on the front of the flashcard. " +
                    "Your task is to generate a clear, relevant, and concise answer that will appear on the back. " +
                    "Generate the answer in the same language as the question, unless the user is explicitly asking for a translation. " +
                    "Keep the answer short and informative to ensure it fits well within the flashcard layout.";
                string model = "gpt-3.5-turbo";
                Entry2 = await _chatGptService.GenerateAiAnswerAsync(requestType, Entry1, model);
            }
        }
        IsLoadingVisible = false;
    }

    [RelayCommand]
    async Task Translate()
    {
        LoadingTitle = "Translating...";
        IsLoadingVisible = true;
        if (EditMode == 1)
        {
            string requestType =
                "You are a helpful assistant for creating flashcards. " +
                "Your task is to translate the text shown on the front of the flashcard. " +
                "Only return the translated text — do not include any extra explanations or formatting. " +
                $"The user's native language is {UserSettingsCopy.NativeLanguage}, and they are studying {secondLanguage}.";

            string model = "gpt-3.5-turbo";
            Entry2 = await _chatGptService.GenerateAiAnswerAsync(requestType, Entry1, model);
        }
        IsLoadingVisible = false;
    }

    [RelayCommand]
    async Task SetLanguage()
    {
        secondLanguage = await Shell.Current.DisplayPromptAsync(
            "Language of the Study Set",
            "Enter the second language you want to use in this set. " +
            "It will be used for automatic translation in bilingual flashcards.");
    }

    [RelayCommand]
    async Task Save()
    {
        if (EditMode == 1)
        {
            flashcard = new Flashcard { Question = Entry1, Answer = Entry2, Img1Name = EntryImg1Name, Img2Name = EntryImg2Name };
            FlashcardRepository.SaveNewFlashcard(flashcard);
        }
        else if (EditMode == 4)
        {
            await CardPackRepository.AddCardPackFromZip(Entry1, Entry2);
        }
        else
        {
            CardPack cardPack = new CardPack { Name = Entry1, Description = Entry2, Language = secondLanguage};
            if (EditMode == 2)
            {
                CardPackRepository.SaveNewCardPack(cardPack);
            }
            else
            {
                CardPackRepository.SaveNewStudySet(cardPack);
            }
        }
        Entry1 = string.Empty;
        Entry2 = string.Empty;
        await GoBack();
    }

    [RelayCommand]
    async Task GoBack()
    {
        if (EditMode == 4)
            await Shell.Current.GoToAsync("../..");
        else
            await Shell.Current.GoToAsync("..");
    }
}
