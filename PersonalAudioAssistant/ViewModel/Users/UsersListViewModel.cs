using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UsersListViewModel : ObservableObject
    {
        private SubUserResponse _selectedUser;
        private readonly ManageCacheData _manageCacheData;

        [ObservableProperty]
        ObservableCollection<SubUserResponse> users;

        public UsersListViewModel(IMemoryCache cache, ManageCacheData manageCacheData)
        {
            _manageCacheData = manageCacheData;
            Users = new ObservableCollection<SubUserResponse>();
        }

        public SubUserResponse SelectedUser
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

        public async Task LoadUsers()
        {
            var usersList = await _manageCacheData.GetUsersAsync();

            Users.Clear();
            foreach (var user in usersList)
            {
                Users.Add(user);
            }
        }

        private async void OnUserSelected(SubUserResponse user)
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
