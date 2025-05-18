using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class AddCardPage : ContentPage
{
    public AddCardPage(AddCardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

}