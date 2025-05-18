using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class CardPage : ContentPage
{
    public CardPage()
    {
        InitializeComponent();
        BindingContext = new CardViewModel();
    }

    protected override void OnAppearing()
    {   // Update the flashcards when navigating to the page
        base.OnAppearing();
        var viewModel = BindingContext as CardViewModel;

        viewModel?.LoadFlashcards();
    }
}
