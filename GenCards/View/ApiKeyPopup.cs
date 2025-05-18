using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

using StudyApp.View;
using StudyApp.Services;
using StudyApp.Models;
using StudyApp.Data;

public class ApiKeyPopup : Popup
{
    private readonly UserSettingsService _settingsService;

    public ApiKeyPopup()
    {
        _settingsService = new UserSettingsService();

        // Set background color dynamically based on theme
        var backgroundColor = Application.Current.RequestedTheme == AppTheme.Dark
            ? (Color)Application.Current.Resources["OffBlack"]
            : (Color)Application.Current.Resources["White"];

        Content = new Border
        {
            StrokeThickness = 1,
            BackgroundColor = backgroundColor,
            Padding = 20,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 15 // Rounded corners
            },
            Shadow = new Shadow
            {
                Brush = Colors.Black,
                Opacity = 0.3f,
                Offset = new Point(3, 3),
                Radius = 10
            },
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = "Add OpenAI API Key",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },

                    new Label
                    {
                        Text = "With ChatGPT, you can create flashcards from photos, generate translations, and get answers to your questions.\n\n" +
                               "To access these features, create your own API key and add it in the settings.",
                        FontSize = 14,
                        HorizontalTextAlignment = TextAlignment.Center
                    },

                    new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            new Button
                            {
                                Text = "Navigate To Settings",
                                CornerRadius = 10,
                                Command = new Command(async () =>
                                {
                                    Close();
                                    await Shell.Current.GoToAsync(nameof(SettingsPage));
                                })
                            },

                            new Button
                            {
                                Text = "Create My API Key",
                                CornerRadius = 10,
                                Command = new Command(async () =>
                                {
                                    Close();
                                    await Launcher.OpenAsync("https://openai.com/api/");
                                    await Shell.Current.GoToAsync(nameof(SettingsPage));
                                })
                            },

                            new Button
                            {
                                Text = "Find Out More",
                                CornerRadius = 10,
                                Command = new Command(async () =>
                                {
                                    Close();
                                    await Launcher.OpenAsync("https://www.merge.dev/blog/chatgpt-api-key");
                                })
                            },

                            new Button
                            {
                                Text = "Remind Me Later",
                                CornerRadius = 10,
                                Command = new Command(() => Close()) // Close popup without action
                            },

                            new Button
                            {
                                Text = "Don't Show Again",
                                CornerRadius = 10,
                                Command = new Command(async () => await DisablePopupAsync())
                            }
                        }
                    }
                }
            }
        };
    }

    private async Task DisablePopupAsync()
    {
        Close();
        UserSettings loaded = await _settingsService.LoadAsync();
        loaded.ShowApiKeyPopup = false;
        UserSettingsCopy.ShowApiKeyPopup = false;
        await _settingsService.SaveAsync(loaded);
    }
}
