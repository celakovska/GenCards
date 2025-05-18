using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class EditCardPage : ContentPage
{
	public EditCardPage()
	{
		InitializeComponent();
        BindingContext = new EditCardViewModel();
    }
}