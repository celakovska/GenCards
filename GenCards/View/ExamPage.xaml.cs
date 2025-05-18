using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class ExamPage : ContentPage
{
	public ExamPage()
	{
		InitializeComponent();
		BindingContext = new ExamViewModel();
	}
}