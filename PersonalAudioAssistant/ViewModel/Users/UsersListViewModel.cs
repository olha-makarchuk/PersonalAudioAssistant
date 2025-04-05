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
        private SubUser _selectedUser;

        [ObservableProperty]
        ObservableCollection<SubUser> users;

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
            Users = new ObservableCollection<SubUser>(); // Ініціалізація колекції
            LoadUsers();
        }

        private async void LoadUsers()
        {
            var usersList = await _mediator.Send(new GetAllUsersOuery());
            Users.Clear();
            foreach (var user in usersList)
            {
                Users.Add(user);
            }
        }

        private async void OnUserSelected(SubUser user)
        {
            await Shell.Current.GoToAsync($"/UpdateUserPage?userId={user.Id}");
            SelectedUser = null;
        }

        [RelayCommand]
        private async Task AddUser()
        {
            await Shell.Current.GoToAsync("/AddUserPage");
        }

        public void RefreshUsers()
        {
            LoadUsers();
        }
    }
}
