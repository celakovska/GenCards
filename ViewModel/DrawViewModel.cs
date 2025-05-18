using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using SkiaSharp;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor;

using StudyApp.View;
using StudyApp.Services;
using StudyApp.Models;

namespace StudyApp.ViewModel;
[QueryProperty(nameof(PicturePath), "imagepath")]
public partial class DrawViewModel : ObservableObject
{
    private readonly ChatGptService _chatGptService;
    [ObservableProperty]
    private bool isLoadingVisible = false;

    private string newImagePath;
    [ObservableProperty]
    private string picturePath;
    [ObservableProperty]
    private ObservableCollection<IDrawingLine> myLines;
    private SKBitmap backgroundBitmap;
    [ObservableProperty]
    private int imgWidth;
    [ObservableProperty]
    private int imgHeight;
    [ObservableProperty]
    private int imageCanvasWidth;
    [ObservableProperty]
    private int imageCanvasHeight;


    public DrawViewModel(ChatGptService chatGptService)
    {
        MyLines = new ObservableCollection<IDrawingLine>();
        _chatGptService = chatGptService;
    }

    partial void OnPicturePathChanged(string value)
    {
        LoadImg();
    }

    private async void LoadImg()
    {
        if (string.IsNullOrEmpty(PicturePath)) return;

        using var backgroundStream = File.OpenRead(PicturePath);
        backgroundBitmap = SKBitmap.Decode(backgroundStream);
        if (backgroundBitmap == null)
        {
            await Toast.Make("Error loading the image.").Show();
            await GoBack();
        }

        int orientation = GetExifOrientation(PicturePath);

        if (orientation == 6 || orientation == 8) // Rotated 90° or 270°
        {
            ImgWidth = backgroundBitmap.Height;
            ImgHeight = backgroundBitmap.Width;
            ImageCanvasWidth = backgroundBitmap.Height;
            ImageCanvasHeight = backgroundBitmap.Width;
        }
        else
        {
            ImgWidth = backgroundBitmap.Width;
            ImgHeight = backgroundBitmap.Height;
            ImageCanvasWidth = backgroundBitmap.Width;
            ImageCanvasHeight = backgroundBitmap.Height;
        }
    }

    private int GetExifOrientation(string path)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(path);
            var exif = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            return exif?.GetInt32(ExifDirectoryBase.TagOrientation) ?? 1;
        }
        catch
        {
            return 1; // Default orientation
        }
    }

    private SKBitmap ApplyExifOrientation(SKBitmap bitmap, int orientation)
    {
        switch (orientation)
        {
            case 3:
                return RotateBitmap(bitmap, 180);
            case 6:
                return RotateBitmap(bitmap, 90);
            case 8:
                return RotateBitmap(bitmap, 270);
            default:
                return bitmap;
        }
    }

    private SKBitmap RotateBitmap(SKBitmap source, float degrees)
    {
        var rotated = new SKBitmap(
            degrees % 180 == 0 ? source.Width : source.Height,
            degrees % 180 == 0 ? source.Height : source.Width);

        using (var canvas = new SKCanvas(rotated))
        {
            canvas.Translate(rotated.Width / 2f, rotated.Height / 2f);
            canvas.RotateDegrees(degrees);
            canvas.Translate(-source.Width / 2f, -source.Height / 2f);
            canvas.DrawBitmap(source, 0, 0);
        }

        return rotated;
    }

    async Task MergeDrawingWithImage()
    {
        using var backgroundStream = File.OpenRead(PicturePath);
        var originalBitmap = SKBitmap.Decode(backgroundStream);

        int orientation = GetExifOrientation(PicturePath);
        using var backgroundBitmap = ApplyExifOrientation(originalBitmap, orientation); // Rotate image if needed

        using var resultBitmap = new SKBitmap(ImageCanvasWidth, ImageCanvasHeight);
        using var canvas = new SKCanvas(resultBitmap);

        var srcRect = new SKRect(0, 0, backgroundBitmap.Width, backgroundBitmap.Height);
        var destRect = new SKRect(0, 0, ImageCanvasWidth, ImageCanvasHeight);

        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(backgroundBitmap, srcRect, destRect);
        canvas.ResetMatrix();

        foreach (var line in MyLines)
        {
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeWidth = 3,
                Color = SKColors.Red
            };

            var path = new SKPath();
            path.MoveTo(line.Points[0].X, line.Points[0].Y);
            for (int i = 1; i < line.Points.Count; i++)
            {
                path.LineTo(line.Points[i].X, line.Points[i].Y);
            }

            canvas.DrawPath(path, paint);
        }

        string imageDirectory = Path.GetDirectoryName(PicturePath);
        string newImageName = Path.GetFileNameWithoutExtension(PicturePath) + "edited.jpg";
        newImagePath = Path.Combine(imageDirectory, newImageName);

        using var fileStream = File.Create(newImagePath);
        using var skImage = SKImage.FromBitmap(resultBitmap);
        using var skData = skImage.Encode(SKEncodedImageFormat.Jpeg, 100);
        skData.SaveTo(fileStream);
    }


    [RelayCommand]
    private void DeleteLines()
    {
        MyLines = new ObservableCollection<IDrawingLine>();
    }

    [RelayCommand]
    async Task SaveDrawing()
    {
        IsLoadingVisible = true;
        await MergeDrawingWithImage();

        // make payload and send to chat gpt
        string model = "gpt-4o-mini";
        string userInput = "Translate the underlined word.";
        string? PayloadImg1 = _chatGptService.ConvertImageToBase64(newImagePath);

        string original_text;
        string translated_text;
        (original_text, translated_text) = await _chatGptService.GenerateAiTranslationAsync(userInput, model, PayloadImg1);

        // TODO resend the request in case of incorrect response format
        // if error presist, cancel the operation, else continue:

        // create a new card and fill it using the response
        Flashcard flashcard = new Flashcard { Question = original_text, Answer = translated_text, Img1Name = newImagePath, Id = Guid.NewGuid() };
        FlashcardRepository.SaveNewFlashcard(flashcard);

        // navidate to edit page for additional changes
        await GoBack();
        await Shell.Current.GoToAsync($"{nameof(EditCardPage)}?IdGuid={flashcard.Id}&Mode=1");
    }

    [RelayCommand]
    async Task GoBack()
    {
        PicturePath = "";
        await Shell.Current.GoToAsync("..");
    }
}