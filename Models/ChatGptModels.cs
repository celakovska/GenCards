namespace StudyApp.Models
{
    public class ChatGptResponse
    { // for standart text responses
        public Choice[]? choices { get; set; }
    }

    public class Choice
    {
        public Message? message { get; set; }
    }

    public class Message
    {
        public string? role { get; set; }
        public string? content { get; set; }
    }

    public class ChatGptTranslation
    { // strict format for translation
        public List<Message>? messages { get; set; }
    }

}
