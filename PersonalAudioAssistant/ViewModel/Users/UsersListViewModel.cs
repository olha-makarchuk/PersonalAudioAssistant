using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UsersListViewModel: ObservableObject
    {
        private readonly IMediator _mediator;

        public UsersListViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [RelayCommand]
        private async Task AddUser()
        {
            await Shell.Current.GoToAsync("//AddUserPage");
        }
    }
}
