using Newtonsoft.Json;
using StudyApp.Data;
using StudyApp.Models;

namespace StudyApp.Services
{
    internal class ProgressRepository
    {
        // load flashcards from file
        public static List<FlashcardProgress> LoadProgressFromFile()
        {
            string filePath = Path.Combine(CurrentData.Instance.CardPackPath, Globals.StudyProgressFile);
            if (!File.Exists(filePath))
            {
                return new List<FlashcardProgress>();
            }

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<FlashcardProgress>>(json) ?? new List<FlashcardProgress>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
                return new List<FlashcardProgress>(); // Return empty list on error
            }
        }

        // save progress changes 
        public static void SaveProgress(FlashcardProgress progressNew)
        {
            if (progressNew != null)
            {
                List<FlashcardProgress> data = LoadProgressFromFile();

                var flashcardProgress = data.FirstOrDefault(x => x.Id == progressNew.Id);
                if (flashcardProgress != null)
                {
                    flashcardProgress.LastReviewDate = progressNew.LastReviewDate;
                    flashcardProgress.NextReviewDate = progressNew.NextReviewDate;
                    flashcardProgress.Stability = progressNew.Stability;
                }
                else
                    data.Add(progressNew);

                SaveProgressToFile(data);
            }
        }

        static void SaveProgressToFile(List<FlashcardProgress> data)
        {
            string filePath = Path.Combine(CurrentData.Instance.CardPackPath, Globals.StudyProgressFile);
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }
    }
}
