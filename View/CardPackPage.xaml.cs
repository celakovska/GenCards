using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class CardPackPage : ContentPage
{
    public CardPackPage()
    {
        InitializeComponent();
        BindingContext = new CardPackViewModel();
    }

    protected override void OnAppearing()
    {   // Update the flashcards when navigating to the page
        base.OnAppearing();
        var viewModel = BindingContext as CardPackViewModel;

        viewModel?.LoadCardPacks();
    }
}
