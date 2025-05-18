namespace StudyApp.Models
{
    public class CardPack
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Language { get; set; }
    }
}
