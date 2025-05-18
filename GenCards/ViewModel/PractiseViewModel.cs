using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using StudyApp.Data;
using StudyApp.Services;
using StudyApp.Models;


namespace StudyApp.ViewModel
{
    public partial class PractiseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string currentPackName;

        private List<Flashcard> listCards = new List<Flashcard>();
        private List<Flashcard> allCards;
        private List<Flashcard> listCardsToRepeat;
        private List<FlashcardProgress> listProgress;

        private Int32 l;    // length
        private Int32 i;

        [ObservableProperty]
        private string question;    // current question
        [ObservableProperty]
        private string answer = ""; // current answer (empty string when the answer is hidden)
        [ObservableProperty]
        string? img1Name;
        [ObservableProperty]
        string? img2Name;

        [ObservableProperty]
        private bool next = false;  // what is next - the question (1) or the answer (0)
        [ObservableProperty]
        private string progress;    // shows practise progress to the user
        [ObservableProperty]
        private string progressLabel = "Progress: ";
        [ObservableProperty]
        private string titleAnswer = "SHOW ANSWER";
        [ObservableProperty]
        private bool answerBtnVisible;
        [ObservableProperty]
        private int selectedRating;
        [ObservableProperty]
        private bool isImg1Vertical = false;
        [ObservableProperty]
        private bool isImg2Vertical = false;

        public PractiseViewModel()
        {
            // Call async method without awaiting to avoid blocking constructor
            Task.Run(async () => await InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            await LoadFlashcardsAsync();
            InitVariables();
        }

        public void InitVariables()
        {
            l = listCards.Count;
            i = 0;
            AnswerBtnVisible = true;
            Question = listCards[i].Question;
            Img1Name = listCards[i].Img1Name;
            IsImg1Vertical = UtilityFunctions.FindImageOrientation(Img1Name);
            Progress = i + 1 + "/" + l;
            Answer = "";
            Img2Name = null;
            IsImg2Vertical = false;
            AnswerBtnVisible = true;
        }

        private async Task LoadAllFlashcards()
        {
            string filepath = Path.Combine(CurrentData.Instance.CardPackPath, Globals.CardPacksFile);
            allCards = new List<Flashcard>();
            if (Globals.Mode == 0)
            {
                CardPack CurrentPack = CardPackRepository.GetCardPackById(CurrentData.Instance.PackID, filepath);
                CurrentPackName = CurrentPack.Name;
                allCards = FlashcardRepository.GetFlashcards();
            }
            else
            {
                List<CardPack> CardPacks = CardPackRepository.GetCardPacks(filepath);
                string flashcardPath;
                for (int i = 0; i < CardPacks.Count; i++)
                {
                    flashcardPath = Path.Combine(CurrentData.Instance.CardPackPath, CardPacks[i].Storage);
                    List<Flashcard> flashcards = FlashcardRepository.LoadFlashcardsFromFile(flashcardPath);
                    allCards.AddRange(flashcards);
                }
            }

            if (allCards.Count == 0)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("No Flashcards Yet",
                    "Create flashcards before starting your practice.", "OK");
                    await GoBack();
                });
                
            }
        }

        private async Task LoadFlashcardsAsync()
        {
            await LoadAllFlashcards();
            listCardsToRepeat = new List<Flashcard>();

            listProgress = ProgressRepository.LoadProgressFromFile();
            if (!listProgress.Any())
            {
                listCards = allCards;
                return;
            }

            DateTime examDate = CountNextExamDate(allCards);

            if (examDate > DateTime.Today)
            {
                await DisplayIdealPractiseDate(examDate);

            }

            for (int i = 0; i < allCards.Count; i++)
            {
                Flashcard flashcard_i = allCards[i];
                var flashcardProgress_i = listProgress.FirstOrDefault(x => x.Id == flashcard_i.Id);
                if (flashcardProgress_i == null)
                {   // no exam history
                    listCards.Add(flashcard_i);
                    continue;
                }
                DateTime nextExam = flashcardProgress_i.NextReviewDate;
                if (nextExam <= examDate)
                {
                    listCards.Add(flashcard_i);
                }
            }

        }

        private async Task DisplayIdealPractiseDate(DateTime examDate)
        {
            int daysUntilNextExam = (examDate - DateTime.Today).Days;
            string nextDate;
            if (daysUntilNextExam == 0)
                nextDate = ($"today");
            else if (daysUntilNextExam == 1)
                nextDate = ($"in 1 day");
            else
                nextDate = ($"in {daysUntilNextExam.ToString()} days");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                bool confirmed = await Shell.Current.DisplayAlert($"Next recommended review is {nextDate}.",
                    "Do you want to continue?", "Yes", "No");
                if (!confirmed)
                {
                    await GoBack();
                }
            });
        }

        private DateTime CountNextExamDate(List<Flashcard> allCards)
        {
            DateTime minDate = DateTime.MaxValue;
            for (int i = 0; i < allCards.Count; i++)
            {
                Flashcard flashcard_i = allCards[i];
                var flashcardProgress_i = listProgress.FirstOrDefault(x => x.Id == flashcard_i.Id);
                if (flashcardProgress_i == null)
                    return DateTime.Today;

                DateTime nextExam = flashcardProgress_i.NextReviewDate;
                if (nextExam <= DateTime.Today)
                {
                    return DateTime.Today;
                }
                else if (nextExam < minDate)
                    minDate = nextExam;
            }
            return minDate;
        }

        private void ShowQuestion()
        {
            Progress = i + 1 + "/" + l;
            Question = listCards[i].Question;
            Img1Name = listCards[i].Img1Name;
            IsImg1Vertical = UtilityFunctions.FindImageOrientation(Img1Name);
            Answer = "";
            Img2Name = null;
            IsImg2Vertical = false;
            AnswerBtnVisible = true;
        }

        private async Task ReviewWrongAnswersAsync()
        {
            listProgress = ProgressRepository.LoadProgressFromFile();
            if (listCardsToRepeat.Count > 0)
            {
                listCards = listCardsToRepeat;
                listCardsToRepeat = new List<Flashcard>();
                InitVariables();
                Next = true;
                ProgressLabel = "Review mistakes: ";
            }
            else
            {
                // end of the practise
                Globals.Mode = 0;
                DisplayCongratulations();
                await GoBack();
            }
        }

        private async void DisplayCongratulations()
        {
            DateTime nextDate = CountNextExamDate(allCards);
            await Shell.Current.DisplayAlert("🎉 Great job!",
              $"You’ve completed today’s practice.\n\n📅 Next session for this pack of flashcards: {nextDate.ToString("dd.MM.yyyy")}",
               "Continue");
        }

        [RelayCommand]
        private async Task ShowQA()
        {   // show the questions and the answers from the flashcards
            if (!Next)
            {   // show the answer
                Answer = listCards[i].Answer;
                Img2Name = listCards[i].Img2Name;
                IsImg2Vertical = UtilityFunctions.FindImageOrientation(Img2Name);
                AnswerBtnVisible = false;
            }

            if (Next)
            {   // show the question
                i++;

                if (i == l)
                {   // questions finished
                    await ReviewWrongAnswersAsync();
                }

                else
                {   // show the next flashcard
                    ShowQuestion();
                }
            }
            Next = !Next;
        }

        [RelayCommand]
        private async Task Rate(string rating)
        {
            var flashcardProgress_i = listProgress.FirstOrDefault(x => x.Id == listCards[i].Id);
            if (flashcardProgress_i == null)
            {
                flashcardProgress_i = new FlashcardProgress
                {
                    Id = listCards[i].Id,
                    PackID = CurrentData.Instance.PackID,
                };
            }

            // update date and retention
            flashcardProgress_i.Review(int.Parse(rating));
            ProgressRepository.SaveProgress(flashcardProgress_i);
            
            // repeat mistakes at the end of practise
            if (rating == "0")
            {
                listCardsToRepeat.Add(listCards[i]);
            }

            await ShowQA();
        }

        [RelayCommand]
        private async Task GoBack()
        {   // Navigate back to the CardPackPage
            Globals.Mode = 0;
            await Shell.Current.GoToAsync("..");
        }
    }
}
