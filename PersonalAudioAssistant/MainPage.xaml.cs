using MediatR;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Services;

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
            if (_authTokenManager == null)
            {
                Console.WriteLine("Error: _authTokenManager is null.");
                return;
            }

            await _authTokenManager.InitializeAsync();

            if (await _authTokenManager.IsSignedInAsync())
            {
                await LoadDataInCache();

                Shell.Current?.GoToAsync("//ProgramPage");
            }
            else
            {
                Shell.Current?.GoToAsync($"//AuthorizationPage");
            }
        }

        private async Task LoadDataInCache()
        {
            var usersList = await _manageCacheData.GetUsersAsync();

            var appSettings = await _manageCacheData.GetAppSettingsAsync();
        }
    }

}