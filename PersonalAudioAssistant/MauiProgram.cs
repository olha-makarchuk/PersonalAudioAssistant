using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Plugin.Maui.Audio;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.ViewModel;
using PersonalAudioAssistant.Views;
using PersonalAudioAssistant.Views.Users;
using PersonalAudioAssistant.ViewModel.Users;
using PersonalAudioAssistant.Platforms;
using Mopups.Hosting;
using Mopups.Interfaces;
using Mopups.Services;
using PersonalAudioAssistant.Views.History;
using PersonalAudioAssistant.ViewModel.History;
using PersonalAudioAssistant.Services.Api;

namespace PersonalAudioAssistant;
public static class MauiProgram
{
    private static IConfiguration _configuration;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        var getAssemebly = Assembly.GetExecutingAssembly();
        using var stream = getAssemebly.GetManifestResourceStream("PersonalAudioAssistant.appsettings.json");

        _configuration = new ConfigurationBuilder().AddJsonStream(stream!).Build();
        builder.Configuration.AddConfiguration(_configuration);

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureMopups()
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
        builder.Services.AddSingleton<IPopupNavigation>(MopupService.Instance);
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<AuthTokenManager>();
        builder.Services.AddSingleton<GoogleUserService>();
        builder.Services.AddScoped<ManageCacheData>();
        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<VoiceApiClient>();
        builder.Services.AddSingleton<PaymentHistoryApiClient>();
        builder.Services.AddSingleton<PaymentApiClient>();
        builder.Services.AddSingleton<AppSettingsApiClient>();
        builder.Services.AddSingleton<AutoPaymentApiClient>();
        builder.Services.AddSingleton<ConversationApiClient>();
        builder.Services.AddSingleton<MainUserApiClient>();

        // Pages
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddScoped<AuthorizationPage, AuthorizationViewModel>();
        builder.Services.AddScoped<RegistrationPage, RegistrationPageViewModel>();
        builder.Services.AddScoped<ProgramPage, ProgramPageViewModel>();
        builder.Services.AddScoped<SettingsPage, SettingsViewModel>();
        builder.Services.AddScoped<UsersListPage, UsersListViewModel>();
        builder.Services.AddScoped<AddUserPage, AddUserViewModel>();
        builder.Services.AddScoped<UpdateUserPage, UpdateUserViewModel>();
        builder.Services.AddScoped<AnaliticsPage, AnaliticsViewModel>();
        builder.Services.AddScoped<PaymentPage, PaymentViewModel>();
        builder.Services.AddScoped<GetAccessToHistoryModalPage, GetAccessToHistoryViewModel>();
        builder.Services.AddScoped<HistoryPage, HistoryViewModel>();
        builder.Services.AddScoped<MessagesPage, MessagesViewModel>();
        builder.Services.AddScoped<MenuPage>();

        builder.Services.AddSingleton<ITextToSpeech>(TextToSpeech.Default);
        builder.Services.AddScoped<SpeechToTextImplementation>();
        builder.Services.AddScoped<ISpeechToText, SpeechToTextImplementation>();


        var app = builder.Build();

        // 1) give MediatorProvider the real IServiceProvider
        MediatorProvider.Services = app.Services;

        // 2) set your SpeechToText.Default to the DI-created instance
        SpeechToText.SetDefault(
            app.Services.GetRequiredService<ISpeechToText>()
        );

        return app;

    }
}
