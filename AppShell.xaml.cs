using StudyApp.View;

namespace StudyApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CardPackPage), typeof(CardPackPage));
            Routing.RegisterRoute(nameof(CardPage), typeof(CardPage));
            Routing.RegisterRoute(nameof(ExamPage), typeof(ExamPage));
            Routing.RegisterRoute(nameof(AddCardPage), typeof(AddCardPage));
            Routing.RegisterRoute(nameof(EditCardPage), typeof(EditCardPage));
            Routing.RegisterRoute(nameof(PractisePage), typeof(PractisePage));
            Routing.RegisterRoute(nameof(DrawPage), typeof(DrawPage));
            Routing.RegisterRoute(nameof(LatexPage), typeof(LatexPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(LoadingPage), typeof(LoadingPage));
        }
    }
}
