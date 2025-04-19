using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Mopups.Services;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;

namespace PersonalAudioAssistant.ViewModel
{
    public partial class GetAccessToHistoryViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly ManageCacheData _manageCacheData;

        [ObservableProperty]
        private ObservableCollection<SubUserResponse> users = new();

        [ObservableProperty]
        private SubUserResponse selectedUser;

        [ObservableProperty]
        private string passwordEntry;

        [ObservableProperty]
        private bool isPasswordExists;

        [ObservableProperty]
        private bool isPasswordCorrect = true;

        public GetAccessToHistoryViewModel(IMediator mediator, ManageCacheData manageCacheData)
        {
            _mediator = mediator;
            _manageCacheData = manageCacheData;

            LoadUsersAsync();
        }

        partial void OnSelectedUserChanged(SubUserResponse value)
        {
            if (value.PasswordHash == null)
            {
                IsPasswordExists = false;
            }
            else
            {
                IsPasswordExists = true;
            }

            IsPasswordCorrect = true;
            passwordEntry = string.Empty;
        }

        public async Task RefreshUsersAsync() => await LoadUsersAsync();

        private async Task LoadUsersAsync()
        {
            var cached = await _manageCacheData.GetUsersAsync();
            Users.Clear();
            foreach (var u in cached)
                Users.Add(u);
        }

        [RelayCommand]
        private async Task CheckPasswordAsync()
        {
            if (SelectedUser == null)
                return;

            // якщо для користувача не встановлено пароль, просто йдемо далі
            if (!IsPasswordExists)
            {
                await OpenHistoryAsync();
                return;
            }

            // перевіряємо пароль через MediatR
            var query = new CheckSubUserPasswordQuery
            {
                UserId = SelectedUser.Id,
                Password = PasswordEntry
            };
            var isCorrect = await _mediator.Send(query);

            IsPasswordCorrect = isCorrect;
            if (!isCorrect)
                return;

            await OpenHistoryAsync();
        }

        private async Task OpenHistoryAsync()
        {
            /*
            // тут вкажіть ваш маршрут до сторінки історії
            await Shell.Current.GoToAsync($"userhistory?userId={SelectedUser.Id}");
            await MopupService.Instance.PopAsync();*/
        }

        [RelayCommand]
        public async Task logout()
        {
            await MopupService.Instance.PopAsync();
        }
    }
}
