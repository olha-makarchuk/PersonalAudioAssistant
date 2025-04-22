using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class UsersListViewModel : ObservableObject
    {
        [ObservableProperty]
        private SubUserResponse selectedUser;

        [ObservableProperty]
        private ObservableCollection<SubUserResponse> users;

        private readonly ManageCacheData _manageCacheData;
        private bool _isNavigating;

        public IAsyncRelayCommand<SubUserResponse> UserSelectedCommand { get; }

        public IAsyncRelayCommand AddUserCommand { get; }

        public UsersListViewModel(ManageCacheData manageCacheData)
        {
            _manageCacheData = manageCacheData;
            Users = new ObservableCollection<SubUserResponse>();

            UserSelectedCommand = new AsyncRelayCommand<SubUserResponse>(UserSelectedAsync, CanExecuteUserSelected);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);

            MessagingCenter.Subscribe<UpdateUserViewModel>(this, "UserUpdated", async (_) =>
            {
                await RefreshUsersAsync();
            });
        }

        private bool CanExecuteUserSelected(SubUserResponse user)
        {
            return user != null && !_isNavigating;
        }

        private async Task UserSelectedAsync(SubUserResponse user)
        {
            if (user == null)
                return;

            _isNavigating = true;
            try
            {
                await Shell.Current.GoToAsync($"/UpdateUserPage?userId={user.Id}");
            }
            finally
            {
                _isNavigating = false;
                SelectedUser = null;
            }
        }

        partial void OnSelectedUserChanged(SubUserResponse value)
        {
            if (value != null && !_isNavigating)
            {
                _ = UserSelectedCommand.ExecuteAsync(value);
            }
        }

        public async Task LoadUsersAsync()
        {
            var usersList = await _manageCacheData.GetUsersAsync();

            Users.Clear();
            foreach (var user in usersList)
            {
                Random random = new Random();
                int randomInt = random.Next(1000);

                user.PhotoPath = $"{user.PhotoPath}?nocache={randomInt}";
                Users.Add(user);
                user.PhotoPath = user.PhotoPath.Split('?')[0];
            }
            OnPropertyChanged(nameof(Users));
        }

        private async Task AddUserAsync()
        {
            await Shell.Current.GoToAsync("/AddUserPage");
        }

        public async Task RefreshUsersAsync()
        {
            await LoadUsersAsync();
        }

        [RelayCommand]
        public void OnNavigatedFrom()
        {
            SelectedUser = null;
        }
    }
}
