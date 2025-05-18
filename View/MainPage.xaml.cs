using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Update the flashcards when navigating to the page
        var viewModel = BindingContext as HomeViewModel;
        viewModel?.LoadStudySets();

        Dispatcher.Dispatch(async () => await viewModel?.CheckApiAsync());
    }
}
