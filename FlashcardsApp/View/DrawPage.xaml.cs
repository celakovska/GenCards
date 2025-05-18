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
                // Pùvodní rozmìry obrázku
                double originalWidth = viewModel.ImgWidth;
                double originalHeight = viewModel.ImgHeight;

                // Pomìr stran obrázku
                double aspectRatio = originalWidth / originalHeight;

                // Aktuální šíøka a výška obrázku
                double displayedWidth = image.Width;
                double displayedHeight = image.Height;

                // Pøizpùsobení podle AspectFit
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
