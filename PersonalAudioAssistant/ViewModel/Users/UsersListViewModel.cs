using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Services;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UsersListViewModel : ObservableObject
    {
        private SubUser _selectedUser;
        private readonly ManageCacheData _manageCacheData;

        [ObservableProperty]
        ObservableCollection<SubUser> users;

        public UsersListViewModel(IMemoryCache cache, ManageCacheData manageCacheData)
        {
            _manageCacheData = manageCacheData;
            Users = new ObservableCollection<SubUser>();
            LoadUsers();
        }

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

        private async void LoadUsers()
        {
            var usersList = await _manageCacheData.GetUsersAsync();

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
