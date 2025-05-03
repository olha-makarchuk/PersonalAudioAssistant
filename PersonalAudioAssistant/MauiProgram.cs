﻿using CommunityToolkit.Maui;
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
using PersonalAudioAssistant.Services.Api.PersonalAudioAssistant.Services.Api;
using PersonalAudioAssistant.Application.Services;
using PersonalAudioAssistant.Application.Interfaces;
using Microcharts.Maui;

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
            .UseMicrocharts() 
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
        builder.Services.AddScoped<AuthApiClient>();
        builder.Services.AddScoped<TokenBase>();
        builder.Services.AddScoped<PasswordManager>();
        builder.Services.AddScoped<ApiClientTokens>();
        builder.Services.AddScoped<ElevenlabsApi>();
        builder.Services.AddScoped<ApiClientGPT>();
        builder.Services.AddScoped<IApiClient, ApiClientVoiceEmbedding>();
        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<VoiceApiClient>();
        builder.Services.AddSingleton<PaymentHistoryApiClient>();
        builder.Services.AddSingleton<PaymentApiClient>();
        builder.Services.AddSingleton<AppSettingsApiClient>();
        builder.Services.AddSingleton<AutoPaymentApiClient>();
        builder.Services.AddSingleton<ConversationApiClient>();
        builder.Services.AddSingleton<MainUserApiClient>();
        builder.Services.AddSingleton<SubUserApiClient>();
        builder.Services.AddSingleton<MessagesApiClient>();
        builder.Services.AddSingleton<MoneyUsedApiClient>();
        builder.Services.AddSingleton<MoneyUsersUsedApiClient>();

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

        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        var app = builder.Build();

        DataProvider.Services = app.Services;

        SpeechToText.SetDefault(
            app.Services.GetRequiredService<ISpeechToText>()
        );

        return app;
    }
}
