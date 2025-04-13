using MediatR;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant
{
    public partial class MainPage : ContentPage
    {
        private AuthTokenManager _authTokenManager;
        private readonly IMediator _mediator;
        private readonly ManageCacheData _manageCacheData;

        public MainPage(IMediator mediator, GoogleUserService googleUserService, ManageCacheData manageCacheData)
        {
            InitializeComponent();
            _mediator = mediator;
            _authTokenManager = new AuthTokenManager(googleUserService, mediator);
            _manageCacheData = manageCacheData;
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
        }
    }
}
