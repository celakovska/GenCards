using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace StudyApp.View;
public partial class LatexPage : ContentPage
{
	public LatexPage()
	{
		InitializeComponent();
    }

    void OnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear();

        var painter = new CSharpMath.SkiaSharp.MathPainter();
        painter.FontSize = 40;
        painter.LaTeX = @"\frac{\sqrt{22}}{3}";
        painter.Draw(canvas);

    }
}