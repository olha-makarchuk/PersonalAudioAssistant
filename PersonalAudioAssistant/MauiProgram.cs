using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Plugin.Maui.Audio;
using PersonalAudioAssistant.Persistence.Context;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.Auth;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.View;
using MediatR;
using PeronalAudioAssistant.Application.PlatformFeatures;
using Microsoft.Extensions.DependencyInjection;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Persistence.Repositories;
using PersonalAudioAssistant.ViewModel;

namespace PersonalAudioAssistant;
public static class MauiProgram
{
    private static IConfiguration _configuration;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Завантаження конфігурації
        var getAssemebly = Assembly.GetExecutingAssembly();
        using var stream = getAssemebly.GetManifestResourceStream("PersonalAudioAssistant.appsettings.json");

        _configuration = new ConfigurationBuilder().AddJsonStream(stream).Build();
        builder.Configuration.AddConfiguration(_configuration);

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Реєстрація інших сервісів
        builder.Services.AddApplication();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton(TextToSpeech.Default);
        builder.Services.AddSingleton(SpeechToText.Default);
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<AuthTokenManager>();
        builder.Services.AddSingleton<GoogleUserService>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<ProgramPage>();
        builder.Services.AddTransient<RegistrationPage>();
        builder.Services.AddScoped<AuthorizationPage, AuthorizationViewModel>();
        builder.Services.AddScoped<RegistrationPage, RegistrationPageViewModel>();

        builder.Services.AddScoped<IMainUserRepository, MainUserRepository>();

        builder.Services.AddTransient<TokenBase>();

        // CosmosDb
        builder.Services.AddCosmos<CosmosDbContext>(_configuration.GetConnectionString("CosmosConnection")!, "AudioAssistantDB");

        return builder.Build();
    }

}
