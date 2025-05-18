namespace StudyApp.Data
{
    public static class Globals
    {
        public static readonly string DataFolder = Path.Combine(FileSystem.AppDataDirectory, "Data");  // app-specific data directory
        public const string StudySetsFile = "StudySets.json";
        public const string CardPacksFile = "CardPacks.json";
        public const string StudyProgressFile = "StudyProgress.json";
        public const string UserSettingsFile = "UserSettings.json";

        public static int Mode = 0;
    }

    public static class UserSettingsCopy
    {
        public static string? ApiKey;
        public static bool ShowApiKeyPopup = true;
        public static string NativeLanguage = "";
    }

    // store the currently selected CardPack and StudySet
    public class CurrentData
    {
        private static CurrentData _instance;
        public static CurrentData Instance => _instance ??= new CurrentData();

        public int StudySetID { get; private set; }
        public int PackID { get; private set; }
        public string CardPackPath { get; private set; }
        public string FlashcardPath { get; private set; }


        private CurrentData()
        {
            // Set the default values
            PackID = 0;
            StudySetID = 0;
            CardPackPath = "";
            FlashcardPath = "";
        }

        // Method to update the selected Pack
        public void UpdateSelectedPack(int selectedPack)
        {
            PackID = selectedPack;
        }

        // Method to update the selected StudySet
        public void UpdateSelectedStudySet(int selectedStudySet)
        {
            StudySetID = selectedStudySet;
        }

        public void UpdateCardPackPath(string newDirectoryPath)
        {
            CardPackPath = newDirectoryPath;
        }

        public void UpdateFlashcardPath(string newDirectoryPath)
        {
            FlashcardPath = newDirectoryPath;
        }
    }


}
