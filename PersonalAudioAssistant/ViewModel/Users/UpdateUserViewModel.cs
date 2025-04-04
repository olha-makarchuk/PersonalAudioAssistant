using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UpdateUserViewModel: ObservableObject
    {
        private readonly IMediator _mediator;

        public UpdateUserViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [RelayCommand]
        private async Task UpdateUser()
        {
            await Shell.Current.GoToAsync("//UsersListPage");
        }

        [RelayCommand]
        private async Task DeleteUser()
        {
            await Shell.Current.GoToAsync("//UsersListPage");
        }
    }
}
