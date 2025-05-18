using Newtonsoft.Json;

using StudyApp.Data;
using StudyApp.Models;


namespace StudyApp.Services
{
    // load flashcards from file, edit, save or delete flashcards
    public static class FlashcardRepository
    {
        // load flashcards from file
        public static List<Flashcard> LoadFlashcardsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<Flashcard>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Flashcard>>(json) ?? new List<Flashcard>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading flashcards: {ex.Message}");
                return new List<Flashcard>(); // Return empty list on error
            }
        }

        // get current in-memory flashcards
        public static List<Flashcard> GetFlashcards()
        {
            string filePath = CurrentData.Instance.FlashcardPath;
            return LoadFlashcardsFromFile(filePath);
        }


        // get a copy of card with corresponding ID 
        public static Flashcard GetFlashcardById(Guid cardId)
        {
            string filePath = CurrentData.Instance.FlashcardPath;
            var flashcard = LoadFlashcardsFromFile(filePath).FirstOrDefault(x => x.Id == cardId);
            if (flashcard != null)
            {
                return new Flashcard
                {
                    Id = flashcard.Id,
                    Question = flashcard.Question,
                    Answer = flashcard.Answer,
                    Img1Name = flashcard.Img1Name,
                    Img2Name = flashcard.Img2Name,
                };
            }
            return null;
        }

        // save flashcard changes 
        public static void UpdateFlashcard(Guid cardId, Flashcard flashcardUpdated)
        {
            if (cardId != flashcardUpdated.Id) return;

            string filePath = CurrentData.Instance.FlashcardPath;
            List<Flashcard> flashcards = LoadFlashcardsFromFile(filePath);

            var flashcard = flashcards.FirstOrDefault(x => x.Id == cardId);
            if (flashcard == null)
                return;

            flashcard.Question = flashcardUpdated.Question;
            flashcard.Answer = flashcardUpdated.Answer;
            flashcard.Img1Name = flashcardUpdated.Img1Name;
            flashcard.Img2Name = flashcardUpdated.Img2Name;
            SaveFlashcardsToFile(flashcards, filePath);
        }

        public static void SaveNewFlashcard(Flashcard flashcardNew)
        {
            if (flashcardNew != null)
            {
                // TODO check if the flashcard already exists
                Flashcard flashcard = new Flashcard();
                if (flashcardNew.Id != Guid.Empty)
                    flashcard.Id = flashcardNew.Id;
                else
                    flashcard.Id = Guid.NewGuid();
                flashcard.Question = flashcardNew.Question;
                flashcard.Answer = flashcardNew.Answer;
                flashcard.Img1Name = flashcardNew.Img1Name;
                flashcard.Img2Name = flashcardNew.Img2Name;

                string filePath = CurrentData.Instance.FlashcardPath;
                List<Flashcard> flashcards = LoadFlashcardsFromFile(filePath);
                flashcards.Add(flashcard);
                SaveFlashcardsToFile(flashcards, filePath);
            }
        }
        public static void SaveFlashcardsToFile(List<Flashcard> flashcards, string filePath)
        {
            string json = JsonConvert.SerializeObject(flashcards, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // detele a flashcard
        public static void DeleteFlashcard(Guid cardId)
        {
            if (cardId == Guid.Empty) return;

            string filePath = CurrentData.Instance.FlashcardPath;
            List<Flashcard> flashcards = LoadFlashcardsFromFile(filePath);

            var flashcardToDelete = flashcards.FirstOrDefault(f => f.Id == cardId);
            if (flashcardToDelete != null)
            {
                DeteleImage(flashcardToDelete.Img1Name);
                DeteleImage(flashcardToDelete.Img2Name);

                flashcards.Remove(flashcardToDelete);
                SaveFlashcardsToFile(flashcards, filePath);
            }

        }
        public static void DeteleImage(string? imgPath)
        {
            if (File.Exists(imgPath))
                File.Delete(imgPath);
        }

    }
}
