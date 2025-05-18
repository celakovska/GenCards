using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using StudyApp.Services;

using StudyApp.View;
using StudyApp.ViewModel;

namespace StudyApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register HttpClient and ChatGptService
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<ChatGptService>();

            builder.Services.AddTransient<AddCardPage>();
            builder.Services.AddTransient<AddCardViewModel>();
            builder.Services.AddTransient<DrawPage>();
            builder.Services.AddTransient<DrawViewModel>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<SettingsViewModel>();

            return builder.Build();
        }
    }
}
