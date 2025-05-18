namespace StudyApp.Models
{
    public class UserSettings
    {
        public string? ApiKey { get; set; }
        public bool ShowApiKeyPopup { get; set; } = true;
        public string NativeLanguage { get; set; } = "English";
    }

}
