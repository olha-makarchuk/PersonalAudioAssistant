using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UpdateUserViewModel : ObservableObject, IQueryAttributable
    {
        private readonly IMediator _mediator;

        [ObservableProperty]
        private string userId;

        public UpdateUserViewModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("userId"))
            {
                //UserId = query["userId"]?.ToString();
            }
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
