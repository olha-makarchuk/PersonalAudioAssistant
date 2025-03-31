using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Plugin.Maui.Audio;
using PersonalAudioAssistant.Persistence.Context;

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

        // Services
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton(TextToSpeech.Default);
        builder.Services.AddSingleton(SpeechToText.Default);
        builder.Services.AddSingleton(AudioManager.Current);

        //CosmosDb
        builder.Services.AddCosmos<CosmosDbContext>(_configuration.GetConnectionString("CosmosConnection")!, "AudioAssistantDB");


        return builder.Build();
	}
}
