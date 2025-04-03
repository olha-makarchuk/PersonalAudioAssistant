using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class ProgramPageViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly AuthTokenManager _authTokenManager;

        public ProgramPageViewModel(IMediator mediator, GoogleUserService googleUserService)
        {
            _mediator = mediator;
            _authTokenManager = new AuthTokenManager(googleUserService, _mediator);
        }

        [RelayCommand]
        private async Task SignOut_Clicked()
        {

            try
            {
                await _authTokenManager.SignOutAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося відкликати токен: {ex.Message}");
            }

            Shell.Current?.GoToAsync("//AuthorizationPage");
        }
    }
}