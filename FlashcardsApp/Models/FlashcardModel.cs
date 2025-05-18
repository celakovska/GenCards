namespace StudyApp.Models
{
    public class Flashcard
    {
        public Guid Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? Img1Name { get; set; }
        public string? Img2Name { get; set; }
    }
}
