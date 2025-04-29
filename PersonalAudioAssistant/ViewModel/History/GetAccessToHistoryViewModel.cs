using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Mopups.Services;
using PersonalAudioAssistant.Contracts.SubUser;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Services.Api.PersonalAudioAssistant.Services.Api;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.History
{
    public partial class GetAccessToHistoryViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly ManageCacheData _manageCacheData;
        private readonly SubUserApiClient _subUserApiClient;

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

        public GetAccessToHistoryViewModel(IMediator mediator, ManageCacheData manageCacheData, SubUserApiClient subUserApiClient)
        {
            _mediator = mediator;
            _manageCacheData = manageCacheData;
            _subUserApiClient = subUserApiClient;
        }

        partial void OnSelectedUserChanged(SubUserResponse value)
        {
            if (value == null)
                return;

            IsPasswordExists = value.passwordHash != null;
            IsPasswordCorrect = true;
            PasswordEntry = string.Empty;
        }


        public async Task LoadUsersAsync()
        {
            SelectedUser = null; 
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

            var isCorrect = await _subUserApiClient.CheckSubUserPasswordAsync(SelectedUser.id, PasswordEntry);

            IsPasswordCorrect = isCorrect;
            if (!isCorrect)
                return;

            await OpenHistoryAsync();
        }

        private async Task OpenHistoryAsync()
        {
            await MopupService.Instance.PopAsync();

            await Shell.Current.GoToAsync($"//HistoryPage?userId={SelectedUser.id}");
        }

        [RelayCommand]
        public async Task CloseMopup()
        {
            await MopupService.Instance.PopAsync();
        }
    }
}
