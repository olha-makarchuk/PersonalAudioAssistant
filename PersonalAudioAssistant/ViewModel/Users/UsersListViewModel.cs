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
        // Автоматично згенерована властивість для SelectedUser
        [ObservableProperty]
        private SubUserResponse selectedUser;

        // Автоматично згенерована властивість для колекції користувачів
        [ObservableProperty]
        private ObservableCollection<SubUserResponse> users;

        private readonly ManageCacheData _manageCacheData;
        private bool _isNavigating;

        // Команда для вибору користувача
        public IAsyncRelayCommand<SubUserResponse> UserSelectedCommand { get; }

        // Команда для додавання користувача
        public IAsyncRelayCommand AddUserCommand { get; }

        // Конструктор
        public UsersListViewModel(IMemoryCache cache, ManageCacheData manageCacheData)
        {
            _manageCacheData = manageCacheData;
            Users = new ObservableCollection<SubUserResponse>();

            // Ініціалізація команд із перевіркою умови виконання
            UserSelectedCommand = new AsyncRelayCommand<SubUserResponse>(UserSelectedAsync, CanExecuteUserSelected);
            AddUserCommand = new AsyncRelayCommand(AddUserAsync);
        }

        // Метод, який перевіряє, чи можна виконати навігацію (вибір користувача)
        private bool CanExecuteUserSelected(SubUserResponse user)
        {
            return user != null && !_isNavigating;
        }

        // Асинхронний метод, що виконує навігацію до сторінки редагування користувача
        private async Task UserSelectedAsync(SubUserResponse user)
        {
            if (user == null)
                return;

            _isNavigating = true;
            try
            {
                // Очікуємо завершення навігації
                await Shell.Current.GoToAsync($"/UpdateUserPage?userId={user.Id}");
            }
            finally
            {
                _isNavigating = false;
                // Після завершення скидаємо вибір користувача
                SelectedUser = null;
            }
        }

        // Викликається автоматично після зміни значення SelectedUser
        partial void OnSelectedUserChanged(SubUserResponse value)
        {
            if (value != null && !_isNavigating)
            {
                // Виклик асинхронної команди без очікування (можна і await, але тут немає потреби через property setter)
                _ = UserSelectedCommand.ExecuteAsync(value);
            }
        }

        // Метод завантаження користувачів із кешу або джерела даних
        public async Task LoadUsersAsync()
        {
            var usersList = await _manageCacheData.GetUsersAsync();

            Users.Clear();
            foreach (var user in usersList)
            {
                Users.Add(user);
            }
        }

        // Асинхронний метод для переходу до сторінки додавання користувача
        private async Task AddUserAsync()
        {
            await Shell.Current.GoToAsync("/AddUserPage");
        }

        // Метод оновлення користувачів
        public async Task RefreshUsersAsync()
        {
            await LoadUsersAsync();
        }
    }
}
