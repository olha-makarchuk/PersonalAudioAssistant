using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Plugin.Maui.Audio;
using PersonalAudioAssistant.Persistence.Context;
using PersonalAudioAssistant.Services;
using PeronalAudioAssistant.Application.PlatformFeatures;
using PersonalAudioAssistant.Application.Interfaces;
using PersonalAudioAssistant.Persistence.Repositories;
using PersonalAudioAssistant.ViewModel;
using PersonalAudioAssistant.Views;
using PersonalAudioAssistant.Views.Users;
using PersonalAudioAssistant.ViewModel.Users;
using PersonalAudioAssistant.Application.Services;
using Microsoft.EntityFrameworkCore;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands;
using PersonalAudioAssistant.Platforms;
using Mopups.Hosting;
using Mopups.Interfaces;
using Mopups.Services;
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
        builder.Services.AddApplication();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton(TextToSpeech.Default);
        builder.Services.AddSingleton(SpeechToText.Default);
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<AuthTokenManager>();
        builder.Services.AddSingleton<GoogleUserService>();
        builder.Services.AddScoped<TokenBase>();
        builder.Services.AddScoped<ManageCacheData>();
        builder.Services.AddScoped<PasswordManager>();
        builder.Services.AddScoped<ElevenlabsApi>();
        builder.Services.AddScoped<ApiClientAudio>();
        builder.Services.AddScoped<WebSocketService>(); 
        builder.Services.AddScoped<ApiClientVoiceEmbedding>(); 
        builder.Services.AddScoped<ApiClientTokens>(); 
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IApiClient, ApiClientVoiceEmbedding>();

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
        builder.Services.AddScoped<MenuPage>();

        // Repositories
        builder.Services.AddScoped<IAudioDataProvider, AndroidAudioDataProvider>();
        builder.Services.AddScoped<IMainUserRepository, MainUserRepository>();
        builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
        builder.Services.AddScoped<ISubUserRepository, SubUserRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        builder.Services.AddScoped<IAutoPaymentRepository, AutoPaymentRepository>();
        builder.Services.AddScoped<IPaymentHistoryRepository, PaymentHistoryRepository>();
        builder.Services.AddScoped<IVoiceRepository, VoiceRepository>();
        builder.Services.AddScoped<ISpeechToText, SpeechToTextImplementation>();
        builder.Services.AddScoped<IBlobStorage, BlobStorage>();
        builder.Services.AddSingleton<BlobStorageConfig>();

        // CosmosDb
        builder.Services.AddDbContext<CosmosDbContext>(options =>
            options.UseCosmos(
                _configuration.GetConnectionString("CosmosConnection")!,
                databaseName: "AudioAssistantDB"),
            ServiceLifetime.Scoped);

        //builder.Services.AddCosmos<CosmosDbContext>(_configuration.GetConnectionString("CosmosConnection")!, "AudioAssistantDB");

        return builder.Build();
    }
}
