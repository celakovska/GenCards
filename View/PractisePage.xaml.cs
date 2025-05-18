using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class PractisePage : ContentPage
{
	public PractisePage()
	{
		InitializeComponent();
        BindingContext = new PractiseViewModel();

    }
}