using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class AddUserViewModel: ObservableObject
    {
        private readonly IMediator _mediator;

        public AddUserViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            await Shell.Current.GoToAsync("//UsersListPage");
        }
    }
}
