using StudyApp.ViewModel;

namespace StudyApp.View;
public partial class DrawPage : ContentPage
{
    public DrawPage(DrawViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnImageSizeChanged(object sender, EventArgs e)
    {
        if (sender is Image image)
        {
            var viewModel = BindingContext as DrawViewModel;
            if (viewModel != null && image.Width > 0 && image.Height > 0)
            {
                // P�vodn� rozm�ry obr�zku
                double originalWidth = viewModel.ImgWidth;
                double originalHeight = viewModel.ImgHeight;

                // Pom�r stran obr�zku
                double aspectRatio = originalWidth / originalHeight;

                // Aktu�ln� ���ka a v��ka obr�zku
                double displayedWidth = image.Width;
                double displayedHeight = image.Height;

                // P�izp�soben� podle AspectFit
                if (displayedWidth / aspectRatio <= displayedHeight)
                {
                    displayedHeight = displayedWidth / aspectRatio;
                }
                else
                {
                    displayedWidth = displayedHeight * aspectRatio;
                }

                // Aktualizace ViewModelu
                viewModel.ImageCanvasWidth = (int)displayedWidth;
                viewModel.ImageCanvasHeight = (int)displayedHeight;
            }
        }
    }

}
