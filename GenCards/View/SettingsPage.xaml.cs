using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }
}