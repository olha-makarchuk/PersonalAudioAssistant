using MediatR;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.VoiceCommands;
using PersonalAudioAssistant.Services;
using System.Threading.Tasks;

namespace PersonalAudioAssistant
{
    public partial class MainPage : ContentPage
    {
        private AuthTokenManager _authTokenManager;
        private readonly IMediator _mediator;
        private readonly IMemoryCache _cache;
        private readonly ManageCacheData _manageCacheData;

        public MainPage(IMediator mediator, GoogleUserService googleUserService, IMemoryCache cache, ManageCacheData manageCacheData)
        {
            InitializeComponent();
            _mediator = mediator;
            _authTokenManager = new AuthTokenManager(googleUserService, mediator);
            _cache = cache;
            _manageCacheData = manageCacheData;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await InitializeApp();
        }

        private async Task InitializeApp()
        {
            var cmd = new CreateVoiceCommand() 
            { 
                Gender = null,
                Name = null,
                Description = null,
                Style = null,
                Age = null,
                URL = null,
                UserId = null,
                VoiceId = null
            };
            await _mediator.Send(cmd);

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
