using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using PersonalAudioAssistant.Domain.Entities;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UsersListViewModel : ObservableObject
    {
        private readonly IMediator _mediator;

        public ObservableCollection<SubUser> Users { get; } = new ObservableCollection<SubUser>();

        private SubUser _selectedUser;
        public SubUser SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                if (value != null)
                {
                    OnUserSelected(value);
                }
            }
        }

        public UsersListViewModel(IMediator mediator)
        {
            _mediator = mediator;
            LoadUsers();
        }

        private async void LoadUsers()
        {
            var usersList = await _mediator.Send(new GetAllUsersOuery());
            foreach (var user in usersList)
            {
                Users.Add(user);
            }
        }

        private async void OnUserSelected(SubUser user)
        {
            await Shell.Current.GoToAsync($"//UserDetailPage?userId={user.Id}");
            SelectedUser = null; 
        }

        [RelayCommand]
        private async Task AddUser()
        {
            await Shell.Current.GoToAsync("//AddUserPage");
        }
    }
}
