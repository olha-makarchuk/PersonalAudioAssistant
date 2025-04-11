using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using PersonalAudioAssistant.Application.PlatformFeatures.Commands.SubUserCommands;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.SubUserQuery;
using PersonalAudioAssistant.Application.PlatformFeatures.Queries.VoiceQuery;
using PersonalAudioAssistant.Domain.Entities;
using PersonalAudioAssistant.Services;
using PersonalAudioAssistant.Views.Users;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace PersonalAudioAssistant.ViewModel.Users
{
    public partial class AddUserViewModel : ObservableObject
    {
        private readonly IMediator _mediator;
        private readonly IAudioManager _audioManager;
        private readonly IAudioRecorder _audioRecorder;
        private readonly ManageCacheData _manageCacheData;
        private Stream _recordedAudioStream;
        private List<Voice> allVoices = new List<Voice>();

        [ObservableProperty]
        private bool isAudioRecorded = false;

        public VoiceFilterModel VoiceFilter { get; } = new VoiceFilterModel();

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private SubUser subUser = new SubUser();

        [ObservableProperty]
        private Voice selectedVoice;

        [ObservableProperty]
        private string selectedVoiceUrl;

        [ObservableProperty]
        private bool isEndPhraseSelected = false;

        [ObservableProperty]
        private bool isEndTimeSelected = true;

        [ObservableProperty]
        private int selectedEndTime = 2;

        [ObservableProperty]
        private ObservableCollection<Voice> voices = new ObservableCollection<Voice>();

        [ObservableProperty]
        private ObservableCollection<int> endTimeOptions = new ObservableCollection<int>(Enumerable.Range(2, 9));

        [ObservableProperty]
        private ObservableCollection<string> accentOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> descriptionOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> ageOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> genderOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private ObservableCollection<string> useCaseOptions = new ObservableCollection<string>();

        [ObservableProperty]
        private bool isPasswordEnabled = false;

        public AddUserViewModel(IMediator mediator, IAudioManager audioManager, ManageCacheData manageCacheData)
        {
            _mediator = mediator;
            _audioManager = audioManager;
            _audioRecorder = _audioManager.CreateRecorder();
            _manageCacheData = manageCacheData;

            LoadVoicesAsync();
        }

        partial void OnSelectedVoiceChanged(Voice value)
        {
            if (value != null)
            {
                SelectedVoiceUrl = value.URL;
            }
        }

        partial void OnIsEndPhraseSelectedChanged(bool value)
        {
            if (value)
            {
                IsEndTimeSelected = false;
            }
        }

        partial void OnIsEndTimeSelectedChanged(bool value)
        {
            if (value)
            {
                IsEndPhraseSelected = false;
            }
        }

        private void ApplyVoiceFilter()
        {
            Voices = VoiceFilter.ApplyFilter(allVoices);
            SelectedVoice = Voices.FirstOrDefault();
        }

        [RelayCommand]
        public async Task CreateUserAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                await Shell.Current.DisplayAlert("Помилка", "Ім'я користувача не може бути порожнім.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(SubUser.StartPhrase))
            {
                await Shell.Current.DisplayAlert("Помилка", "Початкова фраза не може бути порожньою.", "OK");
                return;
            }

            if (IsEndPhraseSelected && string.IsNullOrWhiteSpace(SubUser.EndPhrase))
            {
                await Shell.Current.DisplayAlert("Помилка", "Кінцева фраза обрана, але не заповнена.", "OK");
                return;
            }

            if (IsEndTimeSelected && SelectedEndTime <= 0)
            {
                await Shell.Current.DisplayAlert("Помилка", "Час завершення має бути більше 0.", "OK");
                return;
            }

            if (SelectedVoice == null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, оберіть голос.", "OK");
                return;
            }

            if (_recordedAudioStream == null)
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, запишіть голос.", "OK");
                return;
            }

            // Перевірка пароля лише якщо користувач обрав додавання пароля
            if (IsPasswordEnabled && string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Помилка", "Будь ласка, введіть пароль.", "OK");
                return;
            }

            var command = new AddSubUserCoomand
            {
                UserName = UserName,
                Password = IsPasswordEnabled ? Password : string.Empty,
                StartPhrase = SubUser.StartPhrase,
                EndPhrase = IsEndPhraseSelected ? SubUser.EndPhrase : string.Empty,
                EndTime = IsEndTimeSelected ? SelectedEndTime.ToString() : string.Empty,
                VoiceId = SelectedVoice.VoiceId,
                UserVoice = new byte[0]
            };
            await _mediator.Send(command);

            await _manageCacheData.UpdateUsersList();

            var usersListViewModel = Shell.Current.CurrentPage.Handler.MauiContext.Services.GetService(typeof(UsersListViewModel)) as UsersListViewModel;
            if (usersListViewModel != null)
            {
                usersListViewModel.RefreshUsers();
            }

            await Shell.Current.GoToAsync("//UsersListPage");
        }

        private async void LoadVoicesAsync()
        {
            try
            {
                var userId = await SecureStorage.GetAsync("user_id");
                var voiceList = await _mediator.Send(new GetAllVoicesByUserIdQuery() { UserId = await SecureStorage.GetAsync("user_id") });

                if (voiceList != null)
                {
                    allVoices = voiceList;
                    InitializeFilterOptions(); 
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося завантажити голоси: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        public async Task RecordReferenceVoiceAsync()
        {
            try
            {
                IsAudioRecorded = false;

                if (await Permissions.RequestAsync<Permissions.Microphone>() != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Мікрофон не доступний", "OK");
                    return;
                }

                await _audioRecorder.StartAsync();
                await Task.Delay(5000);
                var recordedAudio = await _audioRecorder.StopAsync();

                if (recordedAudio == null)
                {
                    await Shell.Current.DisplayAlert("Помилка", "Запис не вдалося завершити", "OK");
                    return;
                }

                _recordedAudioStream = recordedAudio.GetAudioStream();

                IsAudioRecorded = true;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Сталася помилка: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        public async Task PlaySelectedVoiceAsync()
        {
            try
            {
                var mediaElement = ((AddUserPage)Shell.Current.CurrentPage).MediaElement;

                if (!string.IsNullOrEmpty(SelectedVoiceUrl))
                {
                    mediaElement.Source = SelectedVoiceUrl;
                }
                if (mediaElement.CurrentState == CommunityToolkit.Maui.Core.Primitives.MediaElementState.Playing)
                    mediaElement.Pause();
                else
                    mediaElement.Play();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Помилка", $"Не вдалося відтворити голос: {ex.Message}", "OK");
            }
        }

        private void InitializeFilterOptions()
        {
            DescriptionOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Description))
                    .Select(v => v.Description)
                    .Distinct()
                    .OrderBy(x => x)
            );

            AgeOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Age))
                    .Select(v => v.Age)
                    .Distinct()
                    .OrderBy(x => x)
            );

            GenderOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.Gender))
                    .Select(v => v.Gender)
                    .Distinct()
                    .OrderBy(x => x)
            );

            UseCaseOptions = new ObservableCollection<string>(
                allVoices
                    .Where(v => !string.IsNullOrWhiteSpace(v.UseCase))
                    .Select(v => v.UseCase)
                    .Distinct()
                    .OrderBy(x => x)
            );

            ApplyVoiceFilter(); 
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.URL;
        }

        public void OnNavigatedFrom()
        {
            UserName = null;
            SubUser = new SubUser();
            SelectedVoice = null;
            SelectedVoiceUrl = null;
            IsEndPhraseSelected = false;
            IsEndTimeSelected = true;
            SelectedEndTime = 2;
            IsPasswordEnabled = false; // скидаємо властивість для пароля

            VoiceFilter.ResetDescriptionFilter();
            VoiceFilter.ResetAgeFilter();
            VoiceFilter.ResetGenderFilter();
            VoiceFilter.ResetUseCaseFilter();

            Voices = new ObservableCollection<Voice>(allVoices);
            SelectedVoice = Voices.FirstOrDefault();
            SelectedVoiceUrl = SelectedVoice?.URL;
        }

        [RelayCommand]
        public void ResetAllFilters()
        {
            VoiceFilter.ResetDescriptionFilter();
            VoiceFilter.ResetAgeFilter();
            VoiceFilter.ResetGenderFilter();
            VoiceFilter.ResetUseCaseFilter();
            ApplyVoiceFilter();
        }
    }
}
