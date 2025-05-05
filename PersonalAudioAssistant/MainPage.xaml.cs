using Google.Apis.Drive.v3.Data;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api;

namespace PersonalAudioAssistant
{
    public partial class MainPage : ContentPage
    {
        private AuthTokenManager _authTokenManager;
        private readonly IMediator _mediator;
        private readonly ManageCacheData _manageCacheData;
        private readonly ConversationApiClient _conversationApiClient;

        public MainPage(IMediator mediator, GoogleUserService googleUserService, ManageCacheData manageCacheData, AuthTokenManager authTokenManager, ConversationApiClient conversationApiClient)
        {
            InitializeComponent();
            _mediator = mediator;
            _authTokenManager = authTokenManager;
            _manageCacheData = manageCacheData;
            _conversationApiClient = conversationApiClient;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeApp();
        }
        private async Task InitializeApp()
        {

            LoadingProgressBar.IsVisible = true;
            LoadingProgressBar.Progress = 0;

            if (_authTokenManager == null)
            {
                Console.WriteLine("Error: _authTokenManager is null.");
                LoadingProgressBar.IsVisible = false;
                LoadingLabel.Text = "Помилка завантаження.";
                return;
            }

            await _authTokenManager.InitializeAsync();
            LoadingProgressBar.Progress = 0.3; 

            if (await _authTokenManager.IsSignedInAsync())
            {
                await LoadDataInCache();
                LoadingProgressBar.Progress = 1.0;

                Shell.Current?.GoToAsync("//ProgramPage");
            }
            else
            {
                Shell.Current?.GoToAsync($"//AuthorizationPage");
            }

            LoadingProgressBar.IsVisible = false;
        }

        private async Task LoadDataInCache()
        {
            LoadingLabel.Text = "Завантаження даних...";
            var usersList = await _manageCacheData.GetUsersAsync(progress =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadingProgressBar.Progress = 0.3 + (0.3 * progress);
                });
            });

            var appSettings = await _manageCacheData.GetAppSettingsAsync(progress =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadingProgressBar.Progress = 0.6 + (0.4 * progress);
                });
            });

            var conversation = await _manageCacheData.GetConversationAsync();
            var payment = await _manageCacheData.GetPaymentAsync();
        }
    }
}
